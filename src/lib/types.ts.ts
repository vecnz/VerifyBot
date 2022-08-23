declare module '@skyra/env-utilities' {
	interface Env {
		CLIENT_PREFIX: string;
		NODE_ENV: 'test' | 'development' | 'production';
	}
}
