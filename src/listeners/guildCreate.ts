import { Listener } from '@sapphire/framework';
import type { Guild } from 'discord.js';

export class GuildJoin extends Listener {
	public async run(guild: Guild) {
		// create two roles in the guild

		// Check if server already in db
		const server = await this.container.db.server.findFirst({
			where: {
				id: guild.id
			}
		});

		if (server) {
			// delete server
			await this.container.db.server.delete({
				where: {
					id: guild.id
				}
			});
		}

		const staffRole = await guild.roles.create({
			name: 'Faculty',
			reason: 'VicVerify Faculty Role'
		});

		const studentRole = await guild.roles.create({
			name: 'Student',
			reason: 'VicVerify Student Role'
		});

		// create entry in the database
		await this.container.db.server.create({
			data: {
				id: guild.id,
				studentRole: studentRole.id,
				staffRole: staffRole.id
			}
		});

		// Get all members in the server
		const members = await guild.members.fetch();
		members.forEach(async (member) => {
			// check if member is verified
			const user = await this.container.db.user.findFirst({
				where: {
					id: member.user.id
				}
			});
			if (user && user?.verified) {
				// add verified role to member
				if (user.isStudent) {
					await member.roles.add(studentRole);
				} else {
					await member.roles.add(staffRole);
				}
			}
		});
	}
}
