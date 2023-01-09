import { addVerifiedRoleToUser, informUserOfError, removeVerifiedRoleFromUser } from '#lib/utils';
import { ApplyOptions } from '@sapphire/decorators';
import { Command } from '@sapphire/framework';

@ApplyOptions<Command.Options>({
	description: 'Complete verification using the verification code.'
})
export class UserCommand extends Command {
	public override registerApplicationCommands(registry: Command.Registry) {
		registry //
			.registerChatInputCommand((builder) =>
				builder //
					.setName(this.name)
					.setDescription(this.description)
					.addStringOption((input) =>
						input //
							.setName('code')
							.setDescription('The code that was emailed to you when you ran the /verify command.')
							.setRequired(true)
					)
			);
	}

	public override async chatInputRun(interaction: Command.ChatInputCommandInteraction) {
		const code = interaction.options.getString('code', true);
		const authorId = interaction.user.id;

		// check if verification has started
		let user = await this.container.db.user.findFirst({ where: { id: authorId } });
		const verificationRecord = (await this.container.db.verificationRecord.findMany({ where: { userId: authorId } })).sort(
			(a, b) => b.createdAt.getTime() - a.createdAt.getTime()
		)[0];

		if (!user || !verificationRecord) {
			await interaction.reply({ content: 'You have not started verification. Please run the /verify command.', ephemeral: true });
			return;
		}

		if (verificationRecord.completed) {
			if (!user.verified) {
				await interaction.reply({ content: 'You have not started verification. Please run the /verify command.', ephemeral: true });
				return;
			}
			await interaction.reply({
				content: 'You have already completed verification. If you wish to re-verify please run `/verify`',
				ephemeral: true
			});
			return;
		}

		if (verificationRecord.createdAt.getTime() + 3600000 < Date.now()) {
			await interaction.reply({
				content:
					'The verification process has ended as you have taken longer then 1 hour to reply. Please run `/verify` again to restart the process.',
				ephemeral: true
			});
			return;
		}

		// check if the code is correct
		if (verificationRecord.code !== code) {
			this.container.logger.info(`Verification failed for user ${authorId} with code ${code} should be ${verificationRecord.code}`);
			await interaction.reply({ content: 'Invalid code. Please try again.', ephemeral: true });
			return;
		}

		await interaction.deferReply({ ephemeral: true });

		// Check if another user has the same email and unverify them
		const otherUser = await this.container.db.user.findFirst({ where: { email: verificationRecord.email } });
		if (otherUser) {
			try {
				removeVerifiedRoleFromUser(otherUser);
				await this.container.db.user.update({ where: { id: otherUser.id }, data: { verified: false, email: null } });
			} catch (error) {
				await informUserOfError(interaction, error, 'removing the verified status from the user previously verified with this email');
				return;
			}
		}

		// add verified role

		// add user to the db
		user = await this.container.db.user.update({
			where: { id: authorId },
			data: {
				verified: true,
				email: verificationRecord.email,
				isStudent: verificationRecord.isStudent
			}
		});

		// mark verification record as completed
		await this.container.db.verificationRecord.update({
			where: { id: verificationRecord.id },
			data: {
				completed: true
			}
		});

		try {
			addVerifiedRoleToUser(user);
		} catch (error) {
			await informUserOfError(interaction, error, 'adding the verified role to you');
		}

		await interaction.editReply({ content: 'You are now verified across all servers using VicVerify.' });
	}
}
