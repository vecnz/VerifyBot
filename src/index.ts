import '#lib/setup';
import { container, LogLevel, SapphireClient } from '@sapphire/framework';
import { PrismaClient } from '@prisma/client';
import * as nodemailer from 'nodemailer';
import { envParseInteger, envParseString } from '@skyra/env-utilities';
import { Partials, GatewayIntentBits } from 'discord.js';
import '@sapphire/plugin-scheduled-tasks/register';

const client = new SapphireClient({
	shards: 'auto',
	allowedMentions: { users: [], roles: [] },
	intents: [GatewayIntentBits.Guilds, GatewayIntentBits.GuildMembers],
	logger: {
		level: envParseString('NODE_ENV') === 'production' ? LogLevel.Info : LogLevel.Debug
	},
	partials: [Partials.Message, Partials.Channel, Partials.Reaction],
	tasks: {
		bull: {
			connection: {
				host: envParseString('REDIS_HOST')
			}
		}
	}
});

const transporter = nodemailer.createTransport({
	host: envParseString('MAIL_SERVER'),
	port: envParseInteger('MAIL_PORT'),
	auth: {
		user: envParseString('MAIL_USERNAME'),
		pass: envParseString('MAIL_PASSWORD')
	},
	from: 'VUW Discord Verification <verify@vec.ac.nz>'
});

transporter.verify((error) => {
	if (error) {
		container.logger.error(error);
	} else {
		container.logger.info('SMTP server connection successful');
	}
});

async function main() {
	try {
		// Connect to the Database
		const db = new PrismaClient();
		await db.$connect();
		container.db = db;
		container.email = transporter;

		// Login to the Discord gateway
		await client.login();
	} catch (error) {
		container.logger.error(error);
		client.destroy();
		await container.db.$disconnect();
		process.exit(1);
	}
}

main().catch(container.logger.error.bind(container.logger));
