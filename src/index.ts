import './lib/setup';
import { container, SapphireClient } from '@sapphire/framework';
import { PrismaClient } from '@prisma/client';

const client = new SapphireClient({
	shards: 'auto',
	partials: ['MESSAGE', 'CHANNEL', 'REACTION'],
	intents: ['GUILDS', 'GUILD_MESSAGES']
});

async function main() {
	try {
		// Connect to the Database
		const db = new PrismaClient();
		await db.$connect();
		container.db = db;

		// Login to the Discord gateway
		await client.login();
	} catch (error) {
		container.logger.error(error);
		client.destroy();
		container.db.$disconnect();
		process.exit(1);
	}
}

main().catch(container.logger.error.bind(container.logger));
