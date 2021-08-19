using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

using iMessengerCoreAPI.Controllers;
using iMessengerCoreAPI.Models;

namespace iMessengerCoreAPI.Tests
{
    public class DialogsControllerTests
    {
        private IHost _server;
        private DialogsController _controller;
        private IEnumerable<IGrouping<Guid, Guid>> _dialogs;

        [SetUp]
        public void Setup()
        {
            _server = Host.CreateDefaultBuilder()
                          .ConfigureWebHostDefaults(options => { options.UseStartup<Startup>(); })
                          .Build();

            RGDialogsClients dialogsClientsProvider = _server.Services.GetRequiredService<RGDialogsClients>();
            _controller = new DialogsController(dialogsClientsProvider);
            _dialogs = dialogsClientsProvider?.Init()
                                              .GroupBy((RGDialogsClients client) => client.IDRGDialog,
                                                       (RGDialogsClients client) => client.IDClient);
        }

        [Test]
        public void TypesRegisteredTest()
        {
            Assert.NotNull(_controller);
            Assert.NotNull(_dialogs);
        }

        [Test]
        public void DialogFoundTest()
        {
            IEnumerable<object> chooseAndConvertAppropriateDialogs(IEnumerable<Guid> participants)
                => _dialogs.Where((IGrouping<Guid, Guid> group) => group.IsSupersetFor(participants))
                           .Select((IGrouping<Guid, Guid> group) => new JsonResult(group.Key).Value);

            foreach (IGrouping<Guid, Guid> group in _dialogs)
            {
                IEnumerable<Guid> singleID = group.Take(1);
                IEnumerable<Guid> twoIDs   = group.Take(2);
                IEnumerable<Guid> AllIDs   = group;
                
                IEnumerable<object> expectedSingleIDResults = chooseAndConvertAppropriateDialogs(singleID);
                IEnumerable<object> expectedTwoIDsResults   = chooseAndConvertAppropriateDialogs(twoIDs);
                IEnumerable<object> expectedAllIDsResults   = chooseAndConvertAppropriateDialogs(AllIDs);

                JsonResult singleIDResult = _controller.GetByClientsIDs(singleID) as JsonResult;
                JsonResult twoIDsResult   = _controller.GetByClientsIDs(twoIDs) as JsonResult;
                JsonResult allIDsResult   = _controller.GetByClientsIDs(AllIDs) as JsonResult;

                Assert.IsNotNull(singleIDResult, $"Result for single ID for dialog {group.Key} was not {nameof(JsonResult)}.");
                Assert.IsNotNull(singleIDResult, $"Result for two first IDs for dialog {group.Key} was not {nameof(JsonResult)}.");
                Assert.IsNotNull(singleIDResult, $"Result for all IDs for dialog {group.Key} was not {nameof(JsonResult)}.");

                Assert.IsTrue(expectedSingleIDResults.Contains(singleIDResult.Value), $"Single ID for dialog {group.Key}.");
                Assert.IsTrue(expectedTwoIDsResults.Contains(twoIDsResult.Value),     $"Two first IDs for dialog {group.Key}.");
                Assert.IsTrue(expectedAllIDsResults.Contains(allIDsResult.Value),     $"All IDs for dialog {group.Key}.");
            }
        }

        [Test]
        public void DialogNotFoundTest()
        {
            JsonResult expectedResult = new JsonResult(Guid.Empty);

            Guid randomID;
            do
            {
                randomID = Guid.NewGuid();
            } while (_dialogs.Any((IEnumerable<Guid> clients) => clients.Contains(randomID)));
            JsonResult randomIDResult = _controller.GetByClientsIDs(new Guid[] { randomID }) as JsonResult;
            Assert.IsNotNull(randomIDResult, $"Result for random ID was not {nameof(JsonResult)}.");
            Assert.AreEqual(expectedResult.Value, randomIDResult.Value, $"Random ID {randomID} was found at dialog {randomIDResult}.");

            if (!_dialogs.Any()) { return; }
            JsonResult tooManyIDsResult = _controller.GetByClientsIDs(_dialogs.First().Append(randomID)) as JsonResult;
            Assert.IsNotNull(tooManyIDsResult, $"Result for too many IDs was not {nameof(JsonResult)}.");
            Assert.AreEqual(expectedResult.Value, tooManyIDsResult.Value, $"Too many clients was found at dialog {tooManyIDsResult}.");

            IEnumerable<Guid> firstDialog = _dialogs.First();
            IEnumerable<Guid> secondDialog = _dialogs.SkipWhile((IEnumerable<Guid> dialog) => !dialog.Except(firstDialog).Any())
                                                     .FirstOrDefault();
            if (secondDialog is null) { return; }
            JsonResult clientsFromDifferentDialogsResult = _controller.GetByClientsIDs(firstDialog.Union(secondDialog)) as JsonResult;
            Assert.IsNotNull(clientsFromDifferentDialogsResult, $"Result for clients from different dialogs was not {nameof(JsonResult)}.");
            Assert.AreEqual(expectedResult.Value, clientsFromDifferentDialogsResult.Value, 
                            $"Union of clients from different dialogs was found at dialog {clientsFromDifferentDialogsResult}");
        }

        [Test]
        public void IncorrectArgsTest()
        {
            IActionResult expectedResult = new BadRequestResult();

            IActionResult nullResult  = _controller.GetByClientsIDs(null);
            IActionResult emptyResult = _controller.GetByClientsIDs(Enumerable.Empty<Guid>());

            Assert.AreEqual(expectedResult.GetType(), nullResult.GetType(),  "Incorrect result when argument is null.");
            Assert.AreEqual(expectedResult.GetType(), emptyResult.GetType(), "Incorrect result when argument is empty.");
        }
    }
}