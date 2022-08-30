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
			await interaction.reply({ content: 'You have not started verification. Please run the /verify command.', ephemeral: true });
			return;
		}

		if (verificationRecord.completed) {
			if (user.verified) {
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
			await interaction.reply({ content: 'Invalid code. Please try again.', ephemeral: true });
			return;
		}

		await interaction.deferReply({ ephemeral: true });

		interaction.client.guilds.cache.forEach(async (guild) => {
			if (!guild.members.cache.has(authorId)) {
				await guild.members.fetch();
			}
			if (guild.members.cache.has(authorId)) {
				const verifiedRole = await this.container.db.server.findFirst({ where: { id: guild.id } });
				let role;
				if (user.isStudent) {
					role = verifiedRole?.studentRole;
				} else {
					role = verifiedRole?.staffRole;
				}
				if (role) {
					await (guild.members.cache.get(authorId) as GuildMember).roles.add(role);
				}
			}
		});

		// Check if another user has the same email and unverify them
		const otherUser = await this.container.db.user.findFirst({ where: { email: verificationRecord.email } });
		if (otherUser) {
			interaction.client.guilds.cache.forEach(async (guild) => {
				if (!guild.members.cache.has(otherUser.id)) {
					await guild.members.fetch();
				}
				if (guild.members.cache.has(otherUser.id) && user.isStudent) {
					const verifiedRole = await this.container.db.server.findFirst({ where: { id: guild.id } });
					let role;
					if (user.isStudent) {
						role = verifiedRole?.studentRole;
					} else {
						role = verifiedRole?.staffRole;
					}

					if (role) {
						await (guild.members.cache.get(otherUser.id) as GuildMember).roles.remove(role);
					}
				}
			});
		}

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
