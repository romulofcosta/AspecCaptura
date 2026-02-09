# Frontend Integration Testing Guide

**Date**: February 9, 2026  
**Purpose**: Test PWA provisioning, sync, and merge features after Phase 3 implementation

---

## 🧪 Frontend Testing Scenarios

### Test Environment Setup

**Prerequisites**:
1. Backend API running at `http://localhost:5069`
2. PWA running at `http://localhost:5000` (or configured BaseAddress)
3. S3 test data uploaded to `cargas/` path
4. Browser with DevTools open (F12)

---

## 🔐 Authentication Tests

### Test 1: Login with Valid Credentials
```
1. Navigate to PWA Login page
2. Username: admin
3. Password: admin
4. Click "Entrar"

Expected:
✅ Success toast notification
✅ Redirected to /home
✅ User session created in localStorage
✅ IndexedDB has provisioned data
```

**Verify in Browser Console**:
```javascript
// Check session
const session = await db.getItem('pwa-inventory-session');
console.log(session);
// Expected: { username: 'admin', unitId: 1 }

// Check provisioned users
const users = await db.getAllFromIndex('items', 'items');
console.log(users.length); // Should be > 0
```

---

### Test 2: Login with Invalid Credentials
```
1. Navigate to PWA Login page
2. Username: admin
3. Password: wrongpassword
4. Click "Entrar"

Expected:
❌ Error toast "Usuário ou senha inválidos"
❌ Not redirected to /home
❌ Session NOT created
```

---

### Test 3: Offline Login (After Provisioning)
```
1. First, login normally to provision users
2. Stop backend API server (or kill localhost:5069)
3. Logout (navigate to /login)
4. Try to login again

Expected:
✅ Login succeeds OFFLINE (no server call)
✅ Uses Argon2id validation from IndexedDB
✅ Session created successfully
✅ No error about connection
```

**This proves**: 100% offline capability after provisioning

---

## 📦 Provisioning Tests

### Test 4: Verify Users Provisioned
```
Browser Console:
```javascript
// Get all users from IndexedDB
const users = await db.getAllFromIndex('users', 'users');
console.log(users);

// Expected:
// [
//   { nomeUsuario: 'admin', primeiroNome: 'Administrador', ... },
//   { nomeUsuario: 'joao.silva', primeiroNome: 'João', ... },
//   { nomeUsuario: 'maria.santos', primeiroNome: 'Maria', ... }
// ]
```

---

### Test 5: Verify Inventory Provisioned
```
Browser Console:
```javascript
// Get all items from IndexedDB
const items = await db.getAllFromIndex('items', 'items');
console.log(`Total items: ${items.length}`);

// Filter by origin
const cargaItems = items.filter(i => i.origem === 'CargaOficial');
const capturaItems = items.filter(i => i.origem === 'CapturaLocal');
console.log(`CargaOficial: ${cargaItems.length}, CapturaLocal: ${capturaItems.length}`);

// Expected:
// Total items: 10+ (from S3 carga)
// CargaOficial: 10, CapturaLocal: 0
```

---

### Test 6: Check Last Update Timestamp
```
Browser Console:
```javascript
// Get last sync timestamp
const lastUpdate = localStorage.getItem('provisioning-last-update');
console.log(`Last update: ${lastUpdate}`);

// Expected: Recent timestamp (within last few minutes)
```

---

## 🔍 Search & Merge Tests

### Test 7: Search for Item by Name
```
On Home page:
1. Click search bar
2. Type "computador" (or part of any item name)
3. Press Enter or wait for auto-search

Expected:
✅ Returns matching items from CargaOficial
✅ Results shown with correct details
✅ Item shows as CargaOficial origin
```

---

### Test 8: Search for Item by Código
```
On Home page:
1. Click search bar
2. Type "INF-001"
3. Press Enter

Expected:
✅ Returns exact item match
✅ Shows: "Computador Dell XPS 13"
✅ Displays all item details
```

---

### Test 9: Create Local Item (CapturaLocal)
```
1. Navigate to Camera page
2. Enter: Nome = "Notebook Pessoal"
3. Enter: Codigo = "TEST-001"
4. Enter: Localizacao = "Sala Pessoal"
5. Click "Capturar"

Expected:
✅ Item saved to IndexedDB with Origem = 'CapturaLocal'
✅ Item appears in search results immediately
✅ Marked as Sincronizado = false
```

**Verify in Console**:
```javascript
const items = await db.getAllFromIndex('items', 'items');
const testItem = items.find(i => i.codigo === 'TEST-001');
console.log(testItem);
// Expected: { origem: 'CapturaLocal', sincronizado: false, ... }
```

---

### Test 10: Merge Logic - CapturaLocal Prioritization
```
1. Create local item: codigo = "INF-001" (same as existing CargaOficial)
2. Search for "INF-001"

Expected:
✅ Returns LOCAL version (CapturaLocal) first
✅ Local item shows user's custom data
✅ CargaOficial version NOT shown (deduplicated)

This proves: User edits are never shadowed
```

---

## 🔄 Sync Tests

### Test 11: Trigger Manual Sync
```
1. Create 2-3 local items (CapturaLocal)
2. Navigate to Sync page
3. Click "Sincronizar Agora"

Expected:
✅ Loading state shown
✅ PUSH phase: Items uploaded to S3 (capturas/)
✅ PULL phase: Latest carga downloaded
✅ Items marked Sincronizado = true
✅ Success message displayed
```

**Verify in Console**:
```javascript
const items = await db.getAllFromIndex('items', 'items');
const syncedItems = items.filter(i => i.sincronizado === true);
console.log(`Synced items: ${syncedItems.length}`);
// Expected: Increase after sync
```

---

### Test 12: Check Sync Status
```
Browser Console:
```javascript
// This would be called by SyncService.GetSyncStatusAsync()
// But manually check:
const items = await db.getAllFromIndex('items', 'items');
const synced = items.filter(i => i.sincronizado).length;
const pending = items.length - synced;
console.log(`Progress: ${synced}/${items.length} (${(synced/items.length*100).toFixed(0)}%)`);

// Expected after successful sync: 100%
```

---

### Test 13: Sync with Network Error (Recovery)
```
1. Create local items
2. Stop backend API
3. Try to sync
4. Observe loading state continues...
5. Restart backend API

Expected:
❌ Sync fails gracefully with error message
✅ Items marked as Sincronizado = false (not synced)
✅ Retry is possible when server comes back
```

---

## 🌐 Integration Flow Tests

### Test 14: Complete User Journey
```
Step 1: Initial Login
  → Navigate to /login
  → Enter admin/admin
  → Observe provisioning (users + inventory)
  → Redirected to /home

Step 2: Browse Inventory
  → Search for items
  → View item details
  → Verify CargaOficial items shown

Step 3: Create Capture
  → Go to Camera page
  → Create new item (CapturaLocal)
  → Verify appears in search

Step 4: Sync Changes
  → Go to Sync page
  → Trigger sync
  → Verify upload & download phases
  → Check item marked as synced

Step 5: Verify Merge
  → Create another item with code matching CargaOficial
  → Search - verify local version prioritized
  → Confirm deduplication working

Expected:
✅ All steps complete without errors
✅ Data persists across page refreshes
✅ Offline functionality preserved
```

---

## 📊 Data Verification Checks

### Check IndexedDB Structure
```javascript
// View all stores
const db = await openDB('pwa-inventory');
console.log(db.store_names); 
// Expected: ['items', 'users']

// Count records per store
const itemCount = await db.count('items');
const userCount = await db.count('users');
console.log(`Items: ${itemCount}, Users: ${userCount}`);
```

---

### Verify Item Structure
```javascript
const items = await db.getAll('items');
const sample = items[0];
console.log(sample);

// Expected fields:
// {
//   id: string (UUID),
//   nome: string,
//   codigo: string,
//   origem: 'CargaOficial' | 'CapturaLocal',
//   sincronizado: boolean,
//   estaRemoto: boolean,
//   dataUltimaSincronizacao: DateTime | null,
//   ...other fields
// }
```

---

### Verify User Structure
```javascript
const users = await db.getAll('users');
const sample = users[0];
console.log(sample);

// Expected fields:
// {
//   nomeUsuario: string,
//   primeiroNome: string,
//   ultimoNome: string,
//   hashSenha: string (Argon2id),
//   idsUnidadesGestoras: number[],
//   unidadeGestoraAtualId: number,
//   dataCriacao: DateTime
// }
```

---

## 🔐 Security Checks

### Test 15: No Credentials in LocalStorage
```javascript
// Check localStorage doesn't contain passwords
console.log(localStorage);

Expected:
✅ No plain text passwords
✅ Only session info and settings
✅ Sensitive data only in IndexedDB (with Argon2id hash)
```

---

### Test 16: API Key Not Visible in Network
```
1. Open DevTools Network tab
2. Login (triggers provisioning)
3. Observe API calls
4. Click on v2 endpoint calls

Expected:
✅ X-Api-Key header visible (this is ok - HTTPS in production)
✅ No plain passwords in request/response
✅ Argon2id hashes only (never plaintext)
✅ Pre-Signed URLs expire in 30 minutes
```

---

## ❌ Error Handling Tests

### Test 17: Handle Missing S3 Data
```
1. Delete S3 test files
2. Logout and login again
3. Observe error handling

Expected:
❌ Error shown gracefully
✅ App doesn't crash
✅ User can retry
✅ Existing local data preserved
```

---

### Test 18: Handle Invalid JSON
```
1. Manually upload invalid JSON to S3 (test file)
2. Call provisioning endpoint
3. Observe parsing error

Expected:
❌ Error caught and logged
✅ App handles gracefully
❌ Invalid data not stored
```

---

## ✅ Comprehensive Test Checklist

**Authentication**:
- [ ] Login with valid credentials
- [ ] Login with invalid credentials
- [ ] Offline login after provisioning
- [ ] Logout clears session

**Provisioning**:
- [ ] Users downloaded from S3
- [ ] Inventory downloaded from S3
- [ ] Data stored in IndexedDB
- [ ] Last update timestamp recorded

**Search & Merge**:
- [ ] Search by name works
- [ ] Search by código works
- [ ] CapturaLocal items created
- [ ] Merge prioritizes CapturaLocal
- [ ] Deduplication by código works
- [ ] No duplicates in search results

**Sync**:
- [ ] Sync uploads CapturaLocal items
- [ ] Sync downloads latest carga
- [ ] Items marked Sincronizado=true
- [ ] Sync status shows progress
- [ ] Error handling works

**Data Integrity**:
- [ ] IndexedDB structure correct
- [ ] Item fields all present
- [ ] User fields all present
- [ ] Origem field set correctly
- [ ] Sync status tracked accurately

**Security**:
- [ ] Credentials not in localStorage
- [ ] API Key required for v2 endpoints
- [ ] Argon2id hashes never plaintext
- [ ] Pre-Signed URLs expire correctly

---

**Status**: Phase 3b Frontend Testing Ready ✅  
**Next**: Execute manual test scenarios in PWA
