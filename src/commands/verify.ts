import { verifyMsg } from '#lib/constants';
import { informUserOfError } from '#lib/utils';
import { ApplyOptions } from '@sapphire/decorators';
import { Command } from '@sapphire/framework';

@ApplyOptions<Command.Options>({
	description: 'Verify your Discord account using you VUW email address.'
})
export class UserCommand extends Command {
	public override registerApplicationCommands(registry: Command.Registry) {
		registry //
			.registerChatInputCommand((builder) =>
				builder //
					.setName(this.name)
					.setDescription(this.description)
					.addStringOption((input) =>
						input //
							.setName('role')
							.setDescription('Your role at the university')
							.setRequired(true)
							.setChoices(
								...[
									{ name: 'Student', value: 'student' },
									{ name: 'Staff', value: 'staff' }
								]
							)
					)
					.addStringOption((input) =>
						input //
							.setName('email')
							.setDescription('VUW email address')
							.setRequired(true)
					)
			);
	}

	public override async chatInputRun(interaction: Command.ChatInputInteraction) {
		const email = interaction.options.getString('email', true);
		const role = interaction.options.getString('role', true) as 'student' | 'staff';

		const authorId = interaction.user.id;

		// check if valid email and end in @myvuw.ac.nz
		if ((role === 'student' && !/^.+@myvuw\.ac\.nz$/.test(email)) || (role === 'staff' && !/^.+@vuw\.ac\.nz$/.test(email))) {
			await interaction.reply({ content: 'Invalid email address. Please use your VUW email address.', ephemeral: true });
			return;
		}

		// check if user is already verified
		const emailLinked = await this.container.db.user.findFirst({ where: { email } });

		// Needs to be expanded on to add checks if the user is verified as that email and provide a way to move verification to a new discord
		if (emailLinked && emailLinked.verified) {
			await interaction.reply({ content: 'This email is already verified email.', ephemeral: true });
			return;
		}

		// check if another user has this email
		const otherUserWithEmail = await this.container.db.user.findFirst({ where: { email } });
		let msg = '';
		if (otherUserWithEmail) {
			msg =
				'\nAnother user has this email. Continuing to verify will unverify this Discord account as an email can only be associated with a single Discord account.';
		}

		// check if > 5 verification records for this email exist in the last 24 hours
		const verificationRecords = await this.container.db.verificationRecord.findMany({
			where: {
				email,
				createdAt: {
					gte: new Date(Date.now() - 24 * 60 * 60 * 1000)
				}
			}
		});

		if (verificationRecords.length > 5) {
			await interaction.reply({
				content: 'This email has had greater then 5 verification attempts in the past 24 hours, please try again later.',
				ephemeral: true
			});
			return;
		}

		// Check if user is already linked to a discord account
		let discordLinked = await this.container.db.user.findFirst({ where: { id: authorId } });

		// Generate verification code (6 char alpha numeric)
		const code = Math.random().toString(36).split('').slice(2, 8).join('');

		// create user if it doesn't already exist
		if (!discordLinked) {
			discordLinked = await this.container.db.user.create({
				data: {
					id: authorId,
					verified: false
				}
			});
		}

		// create verification record
		await this.container.db.verificationRecord.create({
			data: {
				code,
				isStudent: role === 'student',
				email,
				user: { connect: { id: discordLinked.id } }
			}
		});

		// Send verification email code to user
		try {
			await this.container.email.sendMail({
				from: 'verify@vec.ac.nz',
				to: email,
				subject: 'Verify your VUW Discord account',
				html: verifyMsg(code)
			});
		} catch (error) {
			await informUserOfError(interaction, error, 'sending you the verification email');
			return;
		}

		// Reply to user saying verification email has been sent
		await interaction.reply({ content: `Verification email sent to ${email}.${msg}`, ephemeral: true });
	}
}
