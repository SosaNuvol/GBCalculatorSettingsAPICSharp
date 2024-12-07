namespace QUAD.DSM;

public enum DSMEnvelopeCodeEnum
{
	GEN_COMMON_00000 = 0,
	GEN_COMMON_00001 = 1,

	// Security
	API_COMMON_01000 = 01000,
	API_COMMON_01001 = 01001,

	// Appliation Validation
	API_APPVLD_02000 = 02000,
	API_APPVLD_02001 = 02001,

	// Database
	API_DATABASE_03020 = 03020,

	// Facade
	API_FACADE_04000 = 04000,  // General error for facacdes

	// Repo
	API_REPOS_05000 = 05000,  // General error for repos

	// Services
	API_SERVICES_06001 = 06001, // General error for services
}