namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class AccountNotFoundByClientId(string id) : MovimientoException($"Accounts not found for client with Id: {id}");