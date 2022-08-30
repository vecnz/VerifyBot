import { ScheduledTask } from '@sapphire/plugin-scheduled-tasks';

export class DeleteTask extends ScheduledTask {
	public constructor(context: ScheduledTask.Context, options: ScheduledTask.Options) {
		super(context, options);
	}

	public async run(payload: { userId: string; time: number }) {
		// check if user is in the system
		const user = await this.container.db.user.findFirst({ where: { id: payload.userId } });

		if (!user) {
			return;
		}

		// check if user has been updated
		if (user?.updatedAt.getTime() > payload.time) {
			return;
		}

		// delete user
		// delete all verification records for user
		await this.container.db.verificationRecord.deleteMany({
			where: {
				userId: payload.userId
			}
		});

		await this.container.db.user.delete({
			where: {
				id: payload.userId
			}
		});
	}
}

declare module '@sapphire/plugin-scheduled-tasks' {
	interface ScheduledTasks {
		delete: never;
	}
}
