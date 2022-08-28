import { ApplyOptions } from '@sapphire/decorators';
import { Command } from '@sapphire/framework';
import type { GuildMember } from 'discord.js';

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

	public override async chatInputRun(interaction: Command.ChatInputInteraction) {
		const code = interaction.options.getString('code', true);

		const authorId = interaction.user.id;

		// check if verification has started
		const user = await this.container.db.user.findFirst({ where: { id: authorId } });
		const verificationRecord = (await this.container.db.verificationRecord.findMany({ where: { userId: authorId } })).sort(
			(a, b) => b.createdAt.getTime() - a.createdAt.getTime()
		)[0];

		if (!user || !verificationRecord) {
			interaction.reply({ content: 'You have not started verification. Please run the /verify command.', ephemeral: true });
			return;
		}

		if (verificationRecord.completed) {
			if (user.verified) {
				interaction.reply({ content: 'You have not started verification. Please run the /verify command.', ephemeral: true });
				return;
			} else {
				interaction.reply({
					content: 'You have already completed verification. If you wish to re-verify please run `/verify`',
					ephemeral: true
				});
				return;
			}
		}

		// check if the code is correct
		if (verificationRecord.code !== code) {
			interaction.reply({ content: 'Invalid code. Please try again.', ephemeral: true });
			return;
		}

		await interaction.deferReply({ ephemeral: true });

		interaction.client.guilds.cache.forEach(async (guild) => {
			if (!guild.members.cache.has(authorId)) {
				await guild.members.fetch();
			}
			if (guild.members.cache.has(authorId)) {
				const verifiedRole = await this.container.db.server.findFirst({ where: { id: guild.id } });
				const role = user.isStudent ? verifiedRole?.staffRole : verifiedRole?.studentRole;
				if (role) {
					await (guild.members.cache.get(authorId) as GuildMember).roles.add(role);
				}
			}
		});

		// add user to the db
		await this.container.db.user.update({
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

		await interaction.editReply({ content: 'You are now verified across all servers using VicVerify.' });
	}
}
