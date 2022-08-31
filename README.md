# VicVerify
[![All Contributors](https://img.shields.io/badge/all_contributors-1-orange.svg)](#contributors-) [![Code of Conduct](https://img.shields.io/badge/Contributor%20Covenant-v2.0%20adopted-ff69b4.svg)](https://github.com/vecnz/VerifyBot/blob/main/CODE_OF_CONDUCT.md) ![GitHub](https://img.shields.io/github/license/vecnz/VerifyBot)

Discord verification system for Te Herenga Waka - Victoria University of Wellington. You can invite it [here](https://canary.discord.com/api/oauth2/authorize?client_id=953504345800982598&permissions=268435460&scope=applications.commands%20bot).

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


## Contributors ✨

Thanks goes to these wonderful people ([emoji key](https://allcontributors.org/docs/en/emoji-key)):

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<table>
  <tr>
    <td align="center"><a href="https://github.com/BIOS9"><img src="https://avatars.githubusercontent.com/u/15035908?v=4?s=100" width="100px;" alt=""/><br /><sub><b>NightFish</b></sub></a><br /><a href="https://github.com/vecnz/VerifyBot/commits?author=BIOS9" title="Code">💻</a> <a href="#maintenance-BIOS9" title="Maintenance">🚧</a> <a href="#security-BIOS9" title="Security">🛡️</a></td>
    <td align="center"><a href="http://darkflame.dev"><img src="https://avatars.githubusercontent.com/u/31436575?v=4?s=100" width="100px;" alt=""/><br /><sub><b>Leon Bowie</b></sub></a><br /><a href="#maintenance-Darkflame72" title="Maintenance">🚧</a> <a href="https://github.com/vecnz/VerifyBot/commits?author=Darkflame72" title="Code">💻</a> <a href="#infra-Darkflame72" title="Infrastructure (Hosting, Build-Tools, etc)">🚇</a></td>
  </tr>
</table>

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->

This project follows the [all-contributors](https://github.com/all-contributors/all-contributors) specification. Contributions of any kind welcome!

## Privacy Policy

We collect personal information from you, including information about your:
Email address
Discord account ID

We collect your personal information in order to:
verify Te Herenga Waka - Victoria University of Wellington students and link their Discord accounts to their email address.

We keep your information safe by storing it in a private database and only allowing University staff and the Student Interest and Conflict Resolution team to access you linked email address and history of verification attempts.

We keep your information for as long as you remain verified at which point we securely destroy it by securely erasing all data after 30 days from your request.

You have the right to ask for a copy of any personal information we hold about you, and to ask for it to be corrected if you think it is wrong. If you’d like to ask for a copy of your information, or to have it corrected, please contact us at tech@vec.ac.nz.
