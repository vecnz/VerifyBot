import { Listener } from '@sapphire/framework';
import type { GuildBan } from 'discord.js';

export class GuildBanAdd extends Listener {
	public async run(ban: GuildBan) {
		// get the user from the ban
		const user = ban.user.id;

		// check if the user is verified
		const verifiedUser = await this.container.db.user.findFirst({
			where: {
				id: user
			}
		});

		// if the user is verified
		if (!verifiedUser || !verifiedUser.verified || !verifiedUser.email) return;

		// get the server from the ban
		const server = await this.container.db.server.findFirst({
			where: {
				id: ban.guild.id
			}
		});

		// if the server is not in the database
		if (!server) return;

		const banRecord = await this.container.db.ban.findFirst({
			where: {
				email: verifiedUser.email,
				server: {
					id: ban.guild.id
				}
			}
		});

		if (!banRecord) return;

		// create ban record
		await this.container.db.ban.delete({
			where: {
				id: banRecord.id
			}
		});
	}
}
