export const github = '<https://github.com/vecnz/VerifyBot/issues>';

export const verifyMsg = (code: string) =>
	`Kia ora,\n<br><br>\nSomebody is attempting to connect this email account with their Discord account.\n<br><br>\nIf you did not make this request, you can safely ignore and delete this email.\n<br>\nIf this was you, please send the following command to the VerifyBot Discord bot. On some discord clients such as android you may have to type the command in.\n<br><br>\n<b>/verifycode code:${code}</b>\n<br><br>\nThanks,<br>\nVerifyBot.`;
