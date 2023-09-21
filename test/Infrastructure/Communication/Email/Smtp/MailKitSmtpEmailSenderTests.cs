﻿using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Application.Communication.Email;
using SharedKernel.Application.Exceptions;
using SharedKernel.Infrastructure.MailKit.Communication.Email.MailKitSmtp;
using SharedKernel.Testing.Infrastructure;
using Xunit;

namespace SharedKernel.Integration.Tests.Communication.Email.Smtp
{
    [Collection("DockerHook")]
    public class MailKitSmtpEmailSenderTests : InfrastructureTestCase<FakeStartup>
    {
        protected override string GetJsonFile()
        {
            return "Communication/Email/Smtp/appsettings.smtp.json";
        }

        protected override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            return services.AddMailKitSmtp(Configuration);
        }

        [Fact]
        public async Task SendEmailOk()
        {
            var sender = GetRequiredService<IEmailSender>();

            var result = async () => await sender.SendEmailAsync(EmailTestFactory.Create(), CancellationToken.None);

            await result.Should().NotThrowAsync();
        }

        [Fact]
        public async Task SendEmailWithAttachmentOk()
        {
            var sender = GetRequiredService<IEmailSender>();

            var bytes = await GetPhotoBinary();

            var attachment = new MailAttachment("Adjunto.jpg", bytes);

            var result = async () => await sender.SendEmailAsync(EmailTestFactory.Create(attachment), CancellationToken.None);

            await result.Should().NotThrowAsync();
        }

        [Fact]
        public async Task SendEmailWithAttachmentNotFilenameExtensionKo()
        {
            var sender = GetRequiredService<IEmailSender>();

            var bytes = await GetPhotoBinary();

            var attachment = new MailAttachment("Adjunto", bytes);

            var result = async () => await sender.SendEmailAsync(EmailTestFactory.Create(attachment), CancellationToken.None);

            await result.Should().ThrowAsync<EmailException>().WithMessage(ExceptionCodes.EMAIL_ATTACH_EXT);
        }

        private static Task<FileStream> GetPhotoBinary()
        {
            const string path = "Communication/Email/Photo.jpg";
            return Task.FromResult(new FileStream(path, FileMode.Open, FileAccess.Read));
        }

        //[Fact]
        //public async Task SendEmailEmptyPasswordTaskKo()
        //{
        //    var smtp = GetRequiredServiceOnNewScope<IOptions<SmtpSettings>>();
        //    smtp.Value.Password = null;
        //    var sender = new MailKitSmtpEmailSender(smtp);

        //    var bytes = await GetPhotoBinary();

        //    var attachment = new MailAttachment("Adjunto", bytes);

        //    var result = async () => await sender.SendEmailAsync(EmailTestFactory.Create(attachment), CancellationToken.None);

        //    await result.Should().ThrowAsync<EmailException>().WithMessage(ExceptionCodes.SMT_PASS_EMPTY);
        //}

        //[Fact]
        //public async Task SendEmailEmptyPasswordTaskSpanishKo()
        //{
        //    var defaultCulture = Thread.CurrentThread.CurrentUICulture;
        //    Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-ES");

        //    var smtp = GetRequiredServiceOnNewScope<IOptions<SmtpSettings>>();
        //    smtp.Value.Password = null;
        //    var sender = new MailKitSmtpEmailSender(smtp);

        //    var bytes = await GetPhotoBinary();

        //    var attachment = new MailAttachment("Adjunto", bytes);

        //    var result = async () => await sender.SendEmailAsync(EmailTestFactory.Create(attachment), CancellationToken.None);

        //    await result.Should().ThrowAsync<EmailException>().WithMessage(ExceptionCodes.SMT_PASS_EMPTY);

        //    Thread.CurrentThread.CurrentUICulture = defaultCulture;
        //}
    }
}