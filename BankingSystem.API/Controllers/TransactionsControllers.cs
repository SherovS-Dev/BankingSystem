using Microsoft.AspNetCore.Mvc;
using BankingSystem.API.DTOs;
using BankingSystem.API.Interfaces;
using BankingSystem.API.Models;

namespace BankingSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService;
        _logger = logger;
    }

    /// <summary>
    /// Получить информацию о транзакции
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Transaction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Transaction>> GetTransaction(int id)
    {
        try
        {
            var transaction = await _transactionService.GetTransactionAsync(id);
            
            if (transaction == null)
                return NotFound(new { message = "Транзакция не найдена" });

            return Ok(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction {TransactionId}", id);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Получить историю транзакций счета
    /// </summary>
    [HttpGet("account/{accountId}")]
    [ProducesResponseType(typeof(IEnumerable<Transaction>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetAccountTransactions(
        int accountId, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var transactions = await _transactionService.GetAccountTransactionsAsync(accountId, page, pageSize);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for account {AccountId}", accountId);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Пополнение счета
    /// </summary>
    [HttpPost("deposit")]
    [ProducesResponseType(typeof(TransactionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TransactionResultDto>> Deposit([FromBody] DepositRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _transactionService.DepositAsync(
                request.AccountId,
                request.Amount,
                request.Description);

            if (!result.Success)
                return BadRequest(result);

            _logger.LogInformation("Deposit of {Amount} completed for account {AccountId}",
                request.Amount, request.AccountId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing deposit");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Снятие средств
    /// </summary>
    [HttpPost("withdraw")]
    [ProducesResponseType(typeof(TransactionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TransactionResultDto>> Withdraw([FromBody] WithdrawalRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _transactionService.WithdrawAsync(
                request.AccountId,
                request.Amount,
                request.Description);

            if (!result.Success)
                return BadRequest(result);

            _logger.LogInformation("Withdrawal of {Amount} completed for account {AccountId}",
                request.Amount, request.AccountId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing withdrawal");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Перевод средств между счетами
    /// </summary>
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(TransactionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TransactionResultDto>> Transfer([FromBody] TransferRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _transactionService.TransferAsync(
                request.FromAccountId,
                request.ToAccountId,
                request.Amount,
                request.Description);

            if (!result.Success)
                return BadRequest(result);

            _logger.LogInformation("Transfer of {Amount} from account {FromAccountId} to {ToAccountId} completed",
                request.Amount, request.FromAccountId, request.ToAccountId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing transfer");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }
}