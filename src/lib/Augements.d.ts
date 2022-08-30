import type { PrismaClient } from '@prisma/client';
import type { Transporter } from 'nodemailer';

declare module '@sapphire/pieces' {
	interface Container {
		db: PrismaClient;
		email: Transporter;
	}
}
