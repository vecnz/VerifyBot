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

		const studentRole = await guild.roles.create({
			name: 'Student',
			reason: 'VicVerify Student Role'
		});

		const staffRole = await guild.roles.create({
			name: 'Faculty',
			reason: 'VicVerify Faculty Role'
		});
		// create entry in the database
		await this.container.db.server.create({
			data: {
				id: guild.id,
				studentRole: studentRole.id,
				staffRole: staffRole.id
			}
		});
	}
}
