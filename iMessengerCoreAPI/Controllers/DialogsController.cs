using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using iMessengerCoreAPI.Models;

namespace iMessengerCoreAPI.Controllers
{
    /// <summary>
    /// Controller provides methods to interact with dialogs.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class DialogsController : ControllerBase
    {
        private readonly RGDialogsClients _dialogsClientsProvider;

        /// <summary>
        /// Initializes a new instance of <see cref="DialogsController"/> with specified data provider.
        /// </summary>
        /// <param name="dialogsClientsProvider"> The instance of <see cref="RGDialogsClients"/> class provides dialogs data. </param>
        public DialogsController(RGDialogsClients dialogsClientsProvider) => _dialogsClientsProvider = dialogsClientsProvider;

        /// <summary>
        /// Returns ID of the dialog in which clients with each of the specified IDs are participating.
        /// </summary>
        /// <param name="clientsIDs"> Sequence of IDs of clients participating in dialog. </param>
        /// <returns> 
        /// Dialog's ID if dialog with all clients specified by <paramref name="clientsIDs"/> was found; <see cref="Guid.Empty"/> otherwise.
        /// </returns>
        /// <response code="200"> Returns dialog's ID or <see cref="Guid.Empty"/> </response>
        /// <response code="400"> If <paramref name="clientsIDs"/> is null or empty. </response>
        [HttpGet("[action]")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetByClientsIDs([FromQuery]IEnumerable<Guid> clientsIDs)
        {
            if (clientsIDs is null || !clientsIDs.Any()) { return BadRequest(); }

            // It is expected (despite method name) that the data may change over time, so we are have to re-read it again every time.
            // If it is not so, the following two lines can be put into the constructor and the result can be cached in a private field,
            // which will reduce the time of calling the API method.
            IEnumerable<RGDialogsClients> dialogsClients = _dialogsClientsProvider.Init();
            IEnumerable<IGrouping<Guid, Guid>> groupedByDialogID =
                dialogsClients.GroupBy((RGDialogsClients client) => client.IDRGDialog,
                                       (RGDialogsClients client) => client.IDClient);

            Guid result = groupedByDialogID.FirstOrDefault((IGrouping<Guid, Guid> group) => group.IsSupersetFor(clientsIDs))?
                                            .Key ?? Guid.Empty;

            return new JsonResult(result);
        }
    }
}
