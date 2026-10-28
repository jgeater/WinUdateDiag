# Diagnostic Check Status Mapping

## Summary
When a diagnostic check fails or encounters an error, the status assigned depends on the **severity** of the issue and the **type of check**:

## Status Levels

### ✅ PASS
- Check executed successfully and found no issues
- Service is running, disk space is sufficient, etc.

### ⚠️ WARNING
- Check executed but found a **warning condition** (recoverable issue)
- Issue requires attention but is not critical
- System can still function, but may be suboptimal

### ❌ ERROR
- Check executed and found a **critical issue** (unrecoverable/blocking)
- Issue must be resolved for Windows Update to work properly
- OR the check itself failed to execute

---

## Per-Check Status Assignment

### Service Status Checks (wuauserv, BITS, cryptsvc, msiserver)
| Startup Type | Service Status | Diagnostic Status | Reason |
|--------------|----------------|-------------------|--------|
| **Manual** | Running | **PASS** | User-controlled startup is acceptable |
| **Manual** | Not Running | **PASS** | User-controlled, expected behavior |
| **Automatic** | Running | **PASS** | Working as expected |
| **Automatic** | Not Running | **ERROR** | Should be running automatically |
| **Disabled** | Running | **ERROR** | Service should not be enabled if disabled |
| **Disabled** | Not Running | **ERROR** | Service must be enabled |
| Service not found | N/A | **ERROR** | Service missing from system |
| Check fails (exception) | N/A | **ERROR** | Cannot verify service status |

### Disk Space Check
| Condition | Status | Reason |
|-----------|--------|--------|
| < 10 GB free | **ERROR** | Critical - insufficient space for updates |
| 10-20 GB free | **WARNING** | Getting low - may need to free space |
| > 20 GB free | **PASS** | Sufficient space |
| Check fails (exception) | **ERROR** | Cannot determine available space |

### Pending Reboot Check
| Condition | Status | Reason |
|-----------|--------|--------|
| Reboot pending | **WARNING** | System needs restart, but not immediate blocker |
| No reboot pending | **PASS** | System up to date |
| Check fails (exception) | **ERROR** | Cannot determine reboot status |

### Update Database Check
| Condition | Status | Reason |
|-----------|--------|--------|
| Database > 1024 MB | **WARNING** | Large database, may need cleanup |
| Database ≤ 1024 MB | **PASS** | Normal size |
| Check fails (exception) | **WARNING** | Cannot verify database (less critical) |

### Last Windows Update Scan Status
| Condition | Status | Reason |
|-----------|--------|--------|
| No scan/install activity in 7+ days | **ERROR** | Critical - system not actively checking for updates |
| Updates failed in past 24 hours | **ERROR** | Critical - recent failures need investigation |
| No MS updates (KB) in past 60 days | **ERROR** | Critical - system not receiving security updates |
| Successful scan/update | **PASS** | Normal operation |
| Scan activity but no installs | **PASS** | System checking for updates |

### Network Connectivity Check
| Condition | Status | Reason |
|-----------|--------|--------|
| Can reach Microsoft servers | **PASS** | Full connectivity |
| Can reach internet but not Microsoft | **WARNING** | Firewall/proxy issue, but connected |
| No network connectivity | **ERROR** | Critical - cannot reach update servers |
| Check fails (exception) | **WARNING** | Cannot verify connectivity |

### System Integrity Check
| Condition | Status | Reason |
|-----------|--------|--------|
| All critical paths exist | **PASS** | System files intact |
| Missing critical paths | **ERROR** | Critical - system corruption |
| Check fails (exception) | **WARNING** | Cannot verify system integrity |

### Feature Update Blocks Check
| Condition | Status | Reason |
|-----------|--------|--------|
| No restrictions detected | **PASS** | Updates can proceed |
| Feature updates deferred/paused | **WARNING** | By design or policy (not necessarily an error) |
| Update restrictions enforced | **PASS** or **WARNING** | Depends on policy intent |
| Check fails (exception) | **WARNING** | Cannot read policies |

---

## Key Principles

### When Exceptions Occur During Checks

1. **Critical checks** (Service status, Disk space, Scan status, System integrity):
   - Exception → **ERROR status**
   - Reason: Cannot verify critical functionality

2. **Warning-level checks** (Network connectivity, System integrity):
   - Exception → **WARNING status**
   - Reason: Cannot check, but not blocking Windows Update immediately

3. **Information checks** (Feature blocks, Update database):
   - Exception → **WARNING status**
   - Reason: Less critical for immediate operations

### Status Priority in Diagnostics Output

Results are typically displayed as:
1. **ERRORS** first (critical issues requiring immediate action)
2. **WARNINGS** next (issues needing attention)
3. **PASSES** last (successful checks)

### When to Fix Each Status

| Status | Action Required | Timeline |
|--------|-----------------|----------|
| **ERROR** | Fix immediately | Before next update |
| **WARNING** | Address when possible | Before systems get too degraded |
| **PASS** | No action | Monitor for changes |

---

## Registry Storage

When using `--diagnose --store-registry`, results are stored in the registry with status-based prefixes:
- `Pass_0000`, `Pass_0001`, ... (successful checks)
- `Warning_0000`, `Warning_0001`, ... (warnings)
- `Error_0000`, `Error_0001`, ... (errors)
- `Summary_Total`, `Summary_Passed`, `Summary_Warnings`, `Summary_Errors` (statistics)

Location: `HKEY_LOCAL_MACHINE\SOFTWARE\WinUpdateDiag`
