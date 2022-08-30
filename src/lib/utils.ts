import { Command, container } from '@sapphire/framework';
import type { User } from '@prisma/client';
import type { GuildMember } from 'discord.js';
import { github } from './constants';

// add role to user in all guilds
export function addVerifiedRoleToUser(user: User) {
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
					await (guild.members.cache.get(user.id) as GuildMember).roles.add(role);
				} catch (error) {
					container.logger.error(error);
				}
			}
		}
	});
}

export function removeVerifiedRoleFromUser(user: User) {
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
					await (guild.members.cache.get(user.id) as GuildMember).roles.remove(role);
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
