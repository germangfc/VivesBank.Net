namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class AccountNotFoundByClientIdException(string id) : MovimientoException($"Accounts not found for client with Id: {id}");