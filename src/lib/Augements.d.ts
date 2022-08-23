import type { PrismaClient } from '@prisma/client';

declare module '@sapphire/pieces' {
	interface Container {
		db: PrismaClient;
	}
}
