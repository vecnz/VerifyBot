# VicVerify

Discord verification system for Te Herenga Waka - Victoria University of Wellington.

## About
This is a Discord bot that handles verification between Discord accounts and Vic uni accounts. It has the ability to verify students and staff separately and assign roles based on the verification. It is designed to be used by both university clubs and official course servers providing a painless and seamless verification system. This system was designed with privacy in mind with the Victoria Engineering Club not having access to any data and all processes are fully automated.

## How to use

### Verification
The verification process is fairly straightforward and only requires the user to have access to their university email.

1. Run `/verify` in Discord which begins the verification process, you will also need to provide if you are a student and your email address which ends in `@vuw.ac.nz` or `@myvuw.ac.nz`.
2. You will receive a verification code in your email in the following few minutes.
3. Run `/verifycode <code>` in Discord to verify your account which will then be synced across all servers.
4. Done!!!


To unlink your Discord account from your Vic account, run `/unlink` in Discord. This will remove all verification roles and after 1 years time delete all data related to your Discord account.

### Server Administration
Server administration is originally handled by the bot but requires additional configuration. When the the bot joins the server it will create a `Student` and `Staff` role which will be used for verified users. These roles have no permissions and it is up to you to add those permissions. It is recommended to remove send messages from `@everyone` and instead require users to be verified in order to send messages.


If you these roles are not created please kick and re-add the bot to the server. If you do this once roles are created the bot **will** create new roles when it joins.

## Development
This is a fully open-source project coordinated by the Victoria Engineering Club from at their discord found [here](https://discord.gg/vec). If you have any questions or suggestions, please feel free to contact us.

### Requirements
The following tools and software are used for development of this service.
- Docker
- Node.js (v18)
- Yarn (v3)

### Installation

This will install all packages and commit hooks allowing local development.
```bash
yarn install
```

To deploy the full stack which is especially useful for testing, you can use the following command.
```bash
docker compose up -d
```
