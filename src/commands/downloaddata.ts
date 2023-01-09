import { ApplyOptions } from '@sapphire/decorators';
import { Command } from '@sapphire/framework';
import { AttachmentBuilder } from 'discord.js';

@ApplyOptions<Command.Options>({
	description: 'Download all data stored by this service.'
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

	public override async chatInputRun(interaction: Command.ChatInputCommandInteraction) {
		const authorId = interaction.user.id;

		const user = await this.container.db.user.findFirst({ where: { id: authorId } });

		// get all verification history linked to this user
		const verificationHistory = await this.container.db.verificationRecord.findMany({ where: { userId: authorId } });

		// remove the verification code from the history
		verificationHistory.forEach((record) => {
			record.code = 'REDACTED';
		});

		const data = {
			user,
			verificationHistory
		};

		const file = new AttachmentBuilder(Buffer.from(JSON.stringify(data)), { name: 'data.json' });

		await interaction.reply({
			content: 'Attached is all data stored by this service for you.',
			files: [file],
			ephemeral: true
		});
	}
}
