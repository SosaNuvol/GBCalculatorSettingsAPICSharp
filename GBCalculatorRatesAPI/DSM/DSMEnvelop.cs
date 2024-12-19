namespace QUAD.DSM;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public interface IDSMEnvelop {
	[JsonProperty("code")]
	public DSMEnvelopeCodeEnum Code { get; }


    [JsonProperty("statusCode")]
    string? StatusCode { get; }

    [JsonProperty("httpStatus")]
    System.Net.HttpStatusCode? HttpStatus { get; }

    [JsonProperty("errorMessage")]
    string? ErrorMessage { get; }

    [JsonProperty("notes")]
    string? Notes { get; }}

public class DSMEnvelop<T,L>(DSMEnvelopeCodeEnum code, string? statusCode, System.Net.HttpStatusCode? httpStatus, string? errorMessage, string? notes, ILogger<L> logger) : IDSMEnvelop {
	private readonly ILogger<L> _logger = logger;

	public DSMEnvelopeCodeEnum Code { get; private set; } = code;

	public string? StatusCode { get; private set; } = statusCode;

	public System.Net.HttpStatusCode? HttpStatus { get; private set; } = httpStatus;

	public string? ErrorMessage { get; private set; } = errorMessage;

	public string? Notes { get; private set; } = notes;

	[JsonProperty("payload")]
	public required T Payload { get; set; }

	public static DSMEnvelop<T,L> Initialize(ILogger<L> logger) {
		var dsm = new DSMEnvelop<T,L>(DSMEnvelopeCodeEnum.GEN_COMMON_00001, null, null, null, null, logger) { Payload = default! };

		return dsm;
	}

	public DSMEnvelop<T,L> Success(T value) {
		Code = DSMEnvelopeCodeEnum._SUCCESS;
		Payload = value;

		return this;
	}

	public DSMEnvelop<T,L> Error(DSMEnvelopeCodeEnum code, string errorMessage) {
		Code = code;
		ErrorMessage = errorMessage;

		Console.WriteLine($"|| ** An error was generated: {errorMessage}");

		return this;
	}

	public DSMEnvelop<T,L> Error (Exception ex) {
		Code = DSMEnvelopeCodeEnum.API_APPVLD_02000;
		HttpStatus = System.Net.HttpStatusCode.BadRequest;

		Console.WriteLine($"|| ** Error was thrown and captured in DSM: {ex.Message}");

		ErrorMessage = ex.Message;

		return this;
	}

	public DSMEnvelop<T,L> Error(Exception ex, DSMEnvelopeCodeEnum code, string message) {
		Code = code;
		HttpStatus = System.Net.HttpStatusCode.BadRequest;

		Console.WriteLine($"|| ** Exception Error was thrown and captured in DSM: {ex.Message}");
		Console.WriteLine($"|| ** With additional message: {message}");

		ErrorMessage = ex.Message;

		return this;
	}

	public DSMEnvelop<T,L> Warning(DSMEnvelopeCodeEnum code, string notes) {
		Code = code;

		Notes = notes;

		return this;
	}

	public DSMEnvelop<T,L> Rebase (IDSMEnvelop parent) {
		Code = parent.Code;
		ErrorMessage = parent.ErrorMessage;

		return this;
	}
	
}
