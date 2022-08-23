// import { GuildSettings, readSettings, writeSettings } from '#lib/database';
// import { LanguageKeys } from '#lib/i18n/languageKeys';
// import { Events } from '#lib/types/Enums';
// import { floatPromise } from '#utils/common';
// import { Colors } from '#utils/constants';
// import { getStickyRoles } from '#utils/functions';
// import { Listener } from '@sapphire/framework';
// import { GuildMember, MessageEmbed, Permissions } from 'discord.js';

// /**
//  * On member join check if the member is verified or if they need to be verified,
//  */
// export class GuildMemberAdd extends Listener {
// 	public async run(member: GuildMember) {
// 		if (await this.handleVerified(member)) return;
// 		this.container.client.emit(Events.NotMutedMemberAdd, member);
// 	}

// 	private async handleStickyRoles(member: GuildMember) {
// 		const db = this.container.database;
// 	}
// }
