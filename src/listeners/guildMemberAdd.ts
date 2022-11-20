import type { Server } from '@prisma/client';
import { Listener } from '@sapphire/framework';
import type { GuildMember } from 'discord.js';

export class GuildMemberAdd extends Listener {
	public async run(member: GuildMember) {
		if (member.user.bot) return;

		// check if the member is verified
		const user = await this.container.db.user.findFirst({
			where: {
				id: member.user.id
			}
		});

		if (!user || !user.verified) {
			// Send a dm to the member saying they need to verify
			try {
				await member.send(
					`Kia ora ${member.user.username}, you need to verify your account before you can fully interact with this server and other VUW servers.\n\nPlease run \`/verify\` to verify your account.`
				);
			} catch (error) {}
			return;
		}

		// check email in bans
		const ban = await this.container.db.ban.findFirst({
			where: {
				email: user.email as string,
				server: {
					id: member.guild.id
				}
			}
		});

		if (ban) {
			// ban the user
			try {
				await member.send(
					`Kia ora ${member.user.username}, you have been banned from this server by your linked email, please contact a moderator or admin of the server.`
				);
			} catch (error) {}
			await member.ban({ reason: ban.reason });
			return;
		}

		// get the server settings
		const server = (await this.container.db.server.findFirst({
			where: {
				id: member.guild.id
			}
		})) as Server;

		if (user.isStudent) {
			// add the student role to the member
			await member.roles.add(server.studentRole, 'VicVerify, adding student to the role');
		} else {
			// add the staff role to the member
			await member.roles.add(server.staffRole, 'VicVerify, adding staff member to the role');
		}
	}
}
