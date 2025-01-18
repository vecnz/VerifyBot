import { ScheduledTask } from '@sapphire/plugin-scheduled-tasks';

export class SyncTask extends ScheduledTask {
	public constructor(context: ScheduledTask.LoaderContext, options: ScheduledTask.Options) {
		super(context, {
			...options,
			pattern: '0 0 * * *'
		});
	}

	public async run() {
		this.container.logger.info('Running sync task');
		const servers = await this.container.db.server.findMany();

		servers.forEach(async (server) => {
			// get bans for the server
			const bans = await this.container.db.ban.findMany({
				where: {
					server: {
						id: server.id
					}
				}
			});
			// get all users in the server
			const users = await this.container.client.guilds.cache.get(server.id)?.members.fetch();
			if (!users) {
				return;
			}

			// get all unverified users in the server
			const unverifiedUsers = users.filter((user) => {
				return !user.roles.cache.has(server.studentRole) && !user.roles.cache.has(server.staffRole);
			});

			unverifiedUsers.forEach(async (user) => {
				// check if user in db
				const dbUser = await this.container.db.user.findFirst({ where: { id: user.id } });
				if (!dbUser || !dbUser.email) {
					return;
				}

				// check if user is banned
				const ban = bans.find((ban) => ban.email === dbUser.email);
				if (!ban) {
					// check if user is verified
					if (dbUser.verified) {
						if (dbUser.isStudent) {
							await user.roles.add(server.studentRole);
						} else {
							await user.roles.add(server.staffRole);
						}
					}
				}
			});
		});
	}
}

declare module '@sapphire/plugin-scheduled-tasks' {
	interface ScheduledTasks {
		cron: never;
	}
}
