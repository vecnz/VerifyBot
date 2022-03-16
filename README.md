# VerifyBot
A Verification system for Victoria University of Wellington Discord servers.

## Usage

## Development

### Generate keys and passwords

Generate username encryption key
```bash
openssl genrsa -aes256 -out private.pem 2048
openssl rsa -in private.pem -outform pem -pubout -out public.pem
```

Generate username hash code
```bash
openssl rand -base64 64
```

Copy the contents of `.env.example` to `.env` and fill in the config.

### Running
You can run the bot directly using dotnet or you can use the provided `docker-compose.yml` to run inside of docker as that is how it is run in production.
