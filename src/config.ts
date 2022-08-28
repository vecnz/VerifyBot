import { LogLevel } from '@sapphire/framework';
import { envParseString } from '@skyra/env-utilities';
import type { ClientOptions } from 'discord.js';

export const CLIENT_OPTIONS: ClientOptions = {
	allowedMentions: { users: [], roles: [] },
	intents: ['GUILDS', 'GUILD_MEMBERS'],
	logger: {
		level: envParseString('NODE_ENV') === 'production' ? LogLevel.Info : LogLevel.Debug
	}
};
