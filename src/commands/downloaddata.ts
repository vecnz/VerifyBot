import { ApplyOptions } from '@sapphire/decorators';
import { Command } from '@sapphire/framework';
import { MessageAttachment } from 'discord.js';

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

	public override async chatInputRun(interaction: Command.ChatInputInteraction) {
		const authorId = interaction.user.id;

		const user = await this.container.db.user.findFirst({ where: { id: authorId } });

		// get all verification history linked to this user
		const verificationHistory = await this.container.db.verificationRecord.findMany({ where: { userId: authorId } });

		const data = {
			user,
			verificationHistory
		};

		const file = new MessageAttachment(Buffer.from(JSON.stringify(data)), 'data.json');

		await interaction.reply({
			content: 'Attached is all data stored by this service for you.',
			files: [file],
			ephemeral: true
		});
	}
}
