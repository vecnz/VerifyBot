import { ApplyOptions } from '@sapphire/decorators';
import { Command } from '@sapphire/framework';
import type { GuildMember } from 'discord.js';

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
		const user = await this.container.db.user.findFirst({ where: { id: authorId } });

		if (!user) {
			interaction.reply({ content: 'You are not in the system.', ephemeral: true });
			return;
		}

		await interaction.deferReply({ ephemeral: true });

		// add verified role to all shared servers
		interaction.client.guilds.cache.forEach(async (guild) => {
			if (!guild.members.cache.has(authorId)) {
				await guild.members.fetch();
			}
			if (guild.members.cache.has(authorId)) {
				const verifiedRole = await this.container.db.server.findFirst({ where: { id: guild.id } });
				const role = user.isStudent ? verifiedRole?.staffRole : verifiedRole?.studentRole;
				if (role) {
					await (guild.members.cache.get(authorId) as GuildMember).roles.remove(role);
				}
			}
		});

		// remove email from user and set to unverified
		await this.container.db.user.update({
			data: {
				verified: false,
				email: null,
				isStudent: null
			},
			where: {
				id: authorId
			}
		});

		// remove latest verification attempt
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

		await interaction.editReply({
			content:
				'Your Discord account is no longer verified to your VUW account. All data related to your verification will be deleted in 1 years time if you do not re-verify.'
		});
	}
}
