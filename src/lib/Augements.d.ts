import type { PrismaClient } from '@prisma/client';
import type { Transporter } from 'nodemailer';
import type { IntegerString } from '@skyra/env-utilities';

declare module '@sapphire/pieces' {
	interface Container {
		db: PrismaClient;
		email: Transporter;
	}
}

declare module '@skyra/env-utilities' {
	interface Env {
		CLIENT_PREFIX: string;
		NODE_ENV: 'test' | 'development' | 'production';
		MAIL_SERVER: string;
		MAIL_PORT: IntegerString;
		MAIL_USERNAME: string;
		MAIL_PASSWORD: string;
		REDIS_HOST: string;
	}
}
