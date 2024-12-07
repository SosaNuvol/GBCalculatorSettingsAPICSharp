namespace QUAD.DSM;

public interface IDSMEnvelop {
	public DSMEnvelopeCodeEnum Code { get; }

	public string? StatusCode { get; }

	public System.Net.HttpStatusCode? HttpStatus { get; }

	public string? ErrorMessage { get; }

	public string? Notes { get; }
}

public class DSMEnvelop<T>(DSMEnvelopeCodeEnum code, string? statusCode, System.Net.HttpStatusCode? httpStatus, string? errorMessage, string? notes) : IDSMEnvelop {
	public DSMEnvelopeCodeEnum Code { get; private set; } = code;

	public string? StatusCode { get; private set; } = statusCode;

	public System.Net.HttpStatusCode? HttpStatus { get; private set; } = httpStatus;

	public string? ErrorMessage { get; private set; } = errorMessage;

	public string? Notes { get; private set; } = notes;

	public T? Payload { get; private set; }

	public static DSMEnvelop<T> Initialize() {
		var dsm = new DSMEnvelop<T>(DSMEnvelopeCodeEnum.GEN_COMMON_00001, null, null, null, null);

		return dsm;
	}

	public DSMEnvelop<T> Success(T value) {
		Code = DSMEnvelopeCodeEnum.GEN_COMMON_00000;
		Payload = value;

		return this;
	}

	public DSMEnvelop<T> Error(DSMEnvelopeCodeEnum code, string errorMessage) {
		Code = code;
		ErrorMessage = errorMessage;

		return this;
	}

	public DSMEnvelop<T> Error (Exception ex) {
		Code = DSMEnvelopeCodeEnum.API_APPVLD_02000;
		HttpStatus = System.Net.HttpStatusCode.BadRequest;

		ErrorMessage = ex.Message;

		return this;
	}

	public DSMEnvelop<T> Rebase (IDSMEnvelop parent) {
		Code = parent.Code;
		ErrorMessage = parent.ErrorMessage;

		return this;
	}
	
}
