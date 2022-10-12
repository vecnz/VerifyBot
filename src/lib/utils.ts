import { Command, container } from '@sapphire/framework';
import type { User } from '@prisma/client';
import { github } from '#lib/constants';

// add role to user in all guilds
export async function addVerifiedRoleToUser(user: User) {
	if (!user.verified || !user.email) {
		return;
	}
	// get ban records for the linked email
	const bans = await container.db.ban.findMany({
		where: {
			email: user.email
		}
	});
	container.client.guilds.cache.forEach(async (guild) => {
		// check if the guild has a ban record
		const ban = bans.find((ban) => ban.serverId === guild.id);
		if (!ban) {
			if (!guild.members.cache.has(user.id)) {
				await guild.members.fetch();
			}
			if (guild.members.cache.has(user.id)) {
				const verifiedRole = await container.db.server.findFirst({ where: { id: guild.id } });
				let role;
				if (user.isStudent) {
					role = verifiedRole?.studentRole;
				} else {
					role = verifiedRole?.staffRole;
				}
				if (role) {
					try {
						// check if we have permission to add roles
						if (guild.me?.permissions.has('MANAGE_ROLES')) {
							// check if the role exists
							if (guild.roles.cache.has(role)) {
								// check if role is below our highest role
								if (guild.me.roles.highest.comparePositionTo(role) > 0) {
									// check if user already has role
									if (!guild.members.cache.get(user.id)?.roles.cache.has(role)) {
										// add role
										await guild.members.cache.get(user.id)?.roles.add(role);
									}
								} else {
									container.logger.warn(`Role ${role} is above my highest role in guild ${guild.name} (${guild.id})`);
								}
							} else {
								container.logger.warn(`Role ${role} does not exist in guild ${guild.name}`);
							}
						} else {
							container.logger.warn(`Missing permissions to add roles in guild ${guild.name} (${guild.id})`);
						}
					} catch (error) {
						container.logger.error(error);
					}
				}
			}
		}
	});
}

export function removeVerifiedRoleFromUser(user: User) {
	if (user.verified) {
		return;
	}
	container.client.guilds.cache.forEach(async (guild) => {
		if (!guild.members.cache.has(user.id)) {
			await guild.members.fetch();
		}
		if (guild.members.cache.has(user.id)) {
			const verifiedRole = await container.db.server.findFirst({ where: { id: guild.id } });
			let role;
			if (user.isStudent) {
				role = verifiedRole?.studentRole;
			} else {
				role = verifiedRole?.staffRole;
			}
			if (role) {
				try {
					// check if we have permission to remove roles
					if (guild.me?.permissions.has('MANAGE_ROLES')) {
						// check if the role exists
						if (guild.roles.cache.has(role)) {
							// check if role is below our highest role
							if (guild.me.roles.highest.comparePositionTo(role) > 0) {
								// check if user already has role
								if (guild.members.cache.get(user.id)?.roles.cache.has(role)) {
									// remove role
									await guild.members.cache.get(user.id)?.roles.remove(role);
								}
							} else {
								container.logger.warn(`Role ${role} is above my highest role in guild ${guild.name} (${guild.id})`);
							}
						} else {
							container.logger.warn(`Role ${role} does not exist in guild ${guild.name}`);
						}
					} else {
						container.logger.warn(`Missing permissions to remove roles in guild ${guild.name} (${guild.id})`);
					}
				} catch (error) {
					container.logger.error(error);
				}
			}
		}
	});
}

export async function informUserOfError(interaction: Command.ChatInputInteraction, error: unknown, action: string) {
	container.logger.error(error);
	await interaction.reply({
		content: `An error has occurred while ${action}. If this problem persists please raise an issue at our github: ${github}`,
		ephemeral: true
	});
}
