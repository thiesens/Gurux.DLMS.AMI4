# Changes Made to This Fork

## Modified by: @thiesens
**Date**: 2024-01-XX

---

## Planned Changes

### Removal of Duende IdentityServer
This fork will remove the dependency on `Microsoft.AspNetCore.ApiAuthorization.IdentityServer` to avoid licensing issues.

**Reason for Change:**
- The package depends on Duende IdentityServer which requires a commercial license for organizations with:
  - More than 4 employees, OR
  - More than $1M USD annual revenue
- The package is deprecated and not officially supported in .NET 7+
- This project targets .NET 8 but uses a .NET 6 package

### Replacement: OpenIddict
We will replace Duende IdentityServer with **OpenIddict**, an open-source, MIT-licensed OpenID Connect server.

**Benefits:**
- ✅ Free and open-source (MIT License)
- ✅ No commercial restrictions
- ✅ Native .NET 8 support
- ✅ Full OAuth2/OIDC compliance
- ✅ Excellent Blazor WebAssembly support

### Files to be Modified:
- `Gurux.DLMS.AMI/Server/Gurux.DLMS.AMI.Server.csproj` - Update package references
- `Gurux.DLMS.AMI/Server/Program.cs` - Replace IdentityServer configuration with OpenIddict
- `Gurux.DLMS.AMI/Server/appsettings.json` - Remove IdentityServer configuration
- `Gurux.DLMS.AMI/Client/Program.cs` - Update client authentication (if needed)

---

## License Compliance

This fork maintains the original **GNU General Public License v2 (GPLv2)** from Gurux Ltd.

All modifications are documented and source code is provided in accordance with GPL requirements.

**Original Project:** https://github.com/Gurux/Gurux.DLMS.AMI4

---

## Status

- [x] Repository forked
- [x] Feature branch created
- [ ] Duende IdentityServer removed
- [ ] OpenIddict implemented
- [ ] Testing completed
- [ ] Documentation updated
