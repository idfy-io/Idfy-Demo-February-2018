using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using Idfy.RestClient;
using Idfy.RestClient.Models;
using Idfy.RestClient.Controllers;
using Idfy.RestClient.Exceptions;
using Idfy.Demo.Common;
using Serilog;

namespace Testing
{
    class Program
    {
        static void Main(string[] args)
        {

            var logger = new Serilog.LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.ColoredConsole()
                .CreateLogger();

            Log.Logger = logger;
            OAuthToken token = null;


            IdfyRestClientClient client = new IdfyRestClientClient(Credentials.ClientId,Credentials.ClienttSecret);
            
            try
            {
                token=client.Auth.Authorize(scopes:new List<OAuthScope>(){OAuthScope.DOCUMENT_FILE,OAuthScope.DOCUMENT_READ,OAuthScope.DOCUMENT_WRITE});
                
            }
            catch (OAuthProviderException e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
                return;
                
            }

            IDocumentsController documents = client.Documents;

            CreateDocumentRequestWrapper request = new CreateDocumentRequestWrapper
            {
                Title = "Test document",
                Description = "This is an important document",
                ExternalId = Guid.NewGuid().ToString(),
                DataToSign = new DataToSign
                {
                    Base64Content = "VGhpcyB0ZXh0IGNhbiBzYWZlbHkgYmUgc2lnbmVk",
                    FileName = "sample.txt",
                    ConvertToPDF = false
                },
                ContactDetails = new ContactDetails
                {
                    Email = "test@test.com",
                    Url = "https://idfy.io"
                },
                Signers = new List<SignerWrapper>(),
                Advanced = new Advanced
                {
                    Tags = new List<string> { "develop", "fun_with_documents" },
                    Attachments = 0,
                    RequiredSignatures = 0,
                    GetSocialSecurityNumber = false,
                    TimeToLive = new TimeToLive
                    {
                        Deadline = DateTime.Now.AddDays(1),
                        DeleteAfterHours = 1
                    }
                }
            };

            SignerWrapper signer = new SignerWrapper
            {
                ExternalSignerId = Guid.NewGuid().ToString(),
                RedirectSettings = new RedirectSettings(),
                SignatureType = new SignatureType(),
                Ui = new UI(),
                Order = 0,
                Required = false,
                SignerInfo = new SignerInfo()
                {
                    FirstName = "Rune",
                    LastName = "Synnevåg",
                    Email = "rune@synnevag.com",
                    Mobile = new Mobile()
                    {
                        CountryCode = "+47",Number = "99716935",
                    },

                },
                Notifications = new Notifications()
                {
                    Setup = new Setup()
                    {
                        SignatureReceipt =  SignatureReceipt.SENDEMAIL,
                        Request = Request.SENDBOTH,                        
                        Reminder = Reminder.OFF,
                        FinalReceipt = FinalReceipt.OFF,
                        Expired = Expired.OFF,
                        Canceled = Canceled.OFF,

                    },
                    
                },
            };
            signer.RedirectSettings.RedirectMode = RedirectMode.DONOT_REDIRECT;
            signer.SignatureType.Mechanism = Mechanism.PKISIGNATURE;
            signer.SignatureType.OnEacceptUseHandWrittenSignature = false;
            signer.Ui.Dialogs = new Dialogs
            {
                Before = new DialogBefore
                {
                    UseCheckBox = false,
                    Title = "Info",
                    Message =
                        "Please read the contract on the next pages carefully. Pay some extra attention to paragraph 5."
                }
            };
            signer.Ui.Language = Language.EN;
            signer.Ui.Styling = new SignatureStyling
            {
                ColorTheme = ColorTheme.PINK,
                Spinner = Spinner.CUBES
            };
            request.Signers.Add(signer);

            

            try
            {
                client.Auth.UpdateAccessToken(token);
                var result = documents.DocumentsCreateAsync(request).Result;

                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result));
                
            }
            catch (APIException e)
            {
                Console.WriteLine(e);
            };

            Console.ReadLine();
        }
    }
}