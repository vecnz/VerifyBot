import { informUserOfError, removeVerifiedRoleFromUser } from '#lib/utils';
import { ApplyOptions } from '@sapphire/decorators';
import { Command } from '@sapphire/framework';

@ApplyOptions<Command.Options>({
	description: 'Unlink and unverify your discord acocunt.'
})
export class UserCommand extends Command {
	public override registerApplicationCommands(registry: Command.Registry) {
		registry //
			.registerChatInputCommand((builder) =>
				builder //
					.setName(this.name)
					.setDescription(this.description)
			);
	}

	public override async chatInputRun(interaction: Command.ChatInputInteraction) {
		const authorId = interaction.user.id;

		// get user
		let user = await this.container.db.user.findFirst({ where: { id: authorId } });

		if (!user) {
			await interaction.reply({ content: 'You are not in the system.', ephemeral: true });
			return;
		}

		await interaction.deferReply({ ephemeral: true });

		// remove email from user and set to unverified
		user = await this.container.db.user.update({
			data: {
				verified: false,
				email: null,
				isStudent: null
			},
			where: {
				id: authorId
			}
		});

		// remove latest verification attempt if it is not completed
		const verificationRecord = (await this.container.db.verificationRecord.findMany({ where: { userId: authorId } })).sort(
			(a, b) => b.createdAt.getTime() - a.createdAt.getTime()
		)[0];

		if (verificationRecord && !verificationRecord.completed) {
			await this.container.db.verificationRecord.delete({
				where: {
					id: verificationRecord.id
				}
			});
		}

		try {
			removeVerifiedRoleFromUser(user);
		} catch (error) {
			await informUserOfError(interaction, error, 'attempting to unverify you');
			return;
		}

		// create task to run in 30 days to delete all data
		this.container.tasks.create('delete', { userId: authorId, time: Date.now() }, 1000 * 60 * 60 * 24 * 30);

		await interaction.editReply({
			content:
				'Your Discord account is no longer verified to your VUW account. All data related to your verification will be deleted in 30 days time if you do not re-verify.'
		});
	}
}
