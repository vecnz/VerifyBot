import { ApplyOptions } from '@sapphire/decorators';
import { Command } from '@sapphire/framework';

@ApplyOptions<Command.Options>({
	description: 'About this bot and service.'
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
		await interaction.reply(
			'This is a free verification service that verified Te Herenga Waka - Victoria University of Wellington students with their Discord.\nOur privacy policy can be found [here](https://github.com/vecnz/VerifyBot/wiki/Privacy-Policy).'
		);
	}
}
