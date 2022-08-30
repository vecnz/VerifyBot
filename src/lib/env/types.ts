declare module '@skyra/env-utilities' {
	interface Env {
		CLIENT_PREFIX: string;
		NODE_ENV: 'test' | 'development' | 'production';
		MAIL_SERVER: string;
		MAIL_PORT: number;
		MAIL_USERNAME: string;
		MAIL_PASSWORD: string;
	}
}
