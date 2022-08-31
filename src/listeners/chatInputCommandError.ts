import { github } from '#lib/constants';
import { Listener, ChatInputCommandErrorPayload } from '@sapphire/framework';

export class ChatInputCommandError extends Listener {
	public async run(err: Error, { interaction, command }: ChatInputCommandErrorPayload): Promise<void> {
		this.container.logger.error(err);
		await interaction.reply({
			content: `An error occurred while executing the command \`${command.name}\`. If this keeps happening, please raise check our GitHub and raise an issue [here](${github}).`,
			ephemeral: true
		});
	}
}
