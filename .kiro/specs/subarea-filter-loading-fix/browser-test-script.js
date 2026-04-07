/**
 * Browser Console Test Script
 * Correção de Carregamento de Bens por Subárea
 * 
 * INSTRUÇÕES:
 * 1. Abrir a aplicação no navegador
 * 2. Fazer login com usuário válido
 * 3. Abrir DevTools (F12) → Console
 * 4. Copiar e colar este script completo
 * 5. Executar: await runAllTests()
 */

// ============================================================================
// CONFIGURAÇÃO
// ============================================================================

const TEST_CONFIG = {
    testUO: "09",           // UO para testes
    testArea: "001",        // Área para testes
    testSubarea: "001",     // Subárea para testes
    expectedYear: new Date().getFullYear()
};

// ============================================================================
// UTILITÁRIOS
// ============================================================================

function logTest(name, status, message, details = null) {
    const emoji = status === 'PASS' ? '✅' : status === 'FAIL' ? '❌' : '⏳';
    console.log(`${emoji} ${name}: ${message}`);
    if (details) {
        console.log('   Detalhes:', details);
    }
}

function assert(condition, message) {
    if (!condition) {
        throw new Error(`Assertion failed: ${message}`);
    }
}

// ============================================================================
// TESTE 1: VERIFICAÇÃO DE ÍNDICES INDEXEDDB
// ============================================================================

async function test1_VerifyIndexes() {
    console.group('📋 TESTE 1: Verificação de Índices IndexedDB');
    
    try {
        const db = await new Promise((resolve, reject) => {
            const request = indexedDB.open('aspec-captura-db');
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
        
        const tx = db.transaction(['patrimonio'], 'readonly');
        const store = tx.objectStore('patrimonio');
        const indexes = Array.from(store.indexNames);
        
        const expectedIndexes = ['nutomb', 'cdUnid', 'cdUnidNorm', 'esfera', 'cdArea', 'cdSArea'];
        const missingIndexes = expectedIndexes.filter(idx => !indexes.includes(idx));
        
        if (missingIndexes.length === 0) {
            logTest('Índices IndexedDB', 'PASS', 
                `Todos os ${indexes.length} índices presentes`, 
                { indexes });
        } else {
            logTest('Índices IndexedDB', 'FAIL', 
                `Índices ausentes: ${missingIndexes.join(', ')}`, 
                { expected: expectedIndexes, found: indexes });
        }
        
        db.close();
        return missingIndexes.length === 0;
        
    } catch (error) {
        logTest('Índices IndexedDB', 'FAIL', error.message);
        return false;
    } finally {
        console.groupEnd();
    }
}

// ============================================================================
// TESTE 2: VERSÃO DO INDEXEDDB
// ============================================================================

async function test2_VerifyDBVersion() {
    console.group('📋 TESTE 2: Versão do IndexedDB');
    
    try {
        const db = await new Promise((resolve, reject) => {
            const request = indexedDB.open('aspec-captura-db');
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
        
        const version = db.version;
        
        if (version === 11) {
            logTest('Versão IndexedDB', 'PASS', 
                `Versão correta: ${version}`);
        } else {
            logTest('Versão IndexedDB', 'FAIL', 
                `Versão incorreta: ${version} (esperado: 11)`);
        }
        
        db.close();
        return version === 11;
        
    } catch (error) {
        logTest('Versão IndexedDB', 'FAIL', error.message);
        return false;
    } finally {
        console.groupEnd();
    }
}

// ============================================================================
// TESTE 3: MÉTODO getPatrimonioBySubarea
// ============================================================================

async function test3_GetPatrimonioBySubarea() {
    console.group('📋 TESTE 3: Método getPatrimonioBySubarea');
    
    try {
        if (!window.dbInterop || !window.dbInterop.getPatrimonioBySubarea) {
            logTest('getPatrimonioBySubarea', 'FAIL', 
                'Método não encontrado em window.dbInterop');
            return false;
        }
        
        const { testUO, testArea, testSubarea } = TEST_CONFIG;
        
        // Teste com filtros
        const filtered = await dbInterop.getPatrimonioBySubarea(testUO, testArea, testSubarea);
        
        // Validar que todos os itens são da subárea correta
        const allMatch = filtered.every(item => 
            item.cdUnid === testUO && 
            item.cdArea === testArea && 
            item.cdSArea === testSubarea
        );
        
        if (allMatch) {
            logTest('getPatrimonioBySubarea', 'PASS', 
                `${filtered.length} bens retornados, todos da subárea ${testSubarea}`,
                { sample: filtered[0] });
        } else {
            const mismatch = filtered.find(item => 
                item.cdUnid !== testUO || 
                item.cdArea !== testArea || 
                item.cdSArea !== testSubarea
            );
            logTest('getPatrimonioBySubarea', 'FAIL', 
                'Alguns itens não correspondem aos filtros',
                { mismatch });
        }
        
        return allMatch;
        
    } catch (error) {
        logTest('getPatrimonioBySubarea', 'FAIL', error.message);
        return false;
    } finally {
        console.groupEnd();
    }
}

// ============================================================================
// TESTE 4: COMPATIBILIDADE LEGADO (FILTROS NULOS)
// ============================================================================

async function test4_LegacyCompatibility() {
    console.group('📋 TESTE 4: Compatibilidade Legado (Filtros Nulos)');
    
    try {
        const { testUO } = TEST_CONFIG;
        
        // Método legado
        const allUO = await dbInterop.getPatrimonioByUO(testUO);
        
        // Método novo com filtros nulos
        const withNullFilters = await dbInterop.getPatrimonioBySubarea(testUO, null, null);
        
        if (allUO.length === withNullFilters.length) {
            logTest('Compatibilidade Legado', 'PASS', 
                `Ambos os métodos retornam ${allUO.length} bens`,
                { legado: allUO.length, novo: withNullFilters.length });
        } else {
            logTest('Compatibilidade Legado', 'FAIL', 
                'Métodos retornam quantidades diferentes',
                { legado: allUO.length, novo: withNullFilters.length });
        }
        
        return allUO.length === withNullFilters.length;
        
    } catch (error) {
        logTest('Compatibilidade Legado', 'FAIL', error.message);
        return false;
    } finally {
        console.groupEnd();
    }
}

// ============================================================================
// TESTE 5: NORMALIZAÇÃO DE CÓDIGOS
// ============================================================================

async function test5_CodeNormalization() {
    console.group('📋 TESTE 5: Normalização de Códigos (Zeros à Esquerda)');
    
    try {
        // Testar com zeros à esquerda
        const result1 = await dbInterop.getPatrimonioBySubarea("009", "001", "001");
        const result2 = await dbInterop.getPatrimonioBySubarea("9", "1", "1");
        
        if (result1.length === result2.length) {
            logTest('Normalização de Códigos', 'PASS', 
                `Ambas as formas retornam ${result1.length} bens`,
                { comZeros: result1.length, semZeros: result2.length });
        } else {
            logTest('Normalização de Códigos', 'FAIL', 
                'Normalização não está funcionando corretamente',
                { comZeros: result1.length, semZeros: result2.length });
        }
        
        return result1.length === result2.length;
        
    } catch (error) {
        logTest('Normalização de Códigos', 'FAIL', error.message);
        return false;
    } finally {
        console.groupEnd();
    }
}

// ============================================================================
// TESTE 6: PERFORMANCE DE CONSULTA
// ============================================================================

async function test6_QueryPerformance() {
    console.group('📋 TESTE 6: Performance de Consulta');
    
    try {
        const { testUO, testArea, testSubarea } = TEST_CONFIG;
        
        // Benchmark consulta por UO
        const startUO = performance.now();
        const allUO = await dbInterop.getPatrimonioByUO(testUO);
        const timeUO = performance.now() - startUO;
        
        // Benchmark consulta por subárea
        const startSubarea = performance.now();
        const filtered = await dbInterop.getPatrimonioBySubarea(testUO, testArea, testSubarea);
        const timeSubarea = performance.now() - startSubarea;
        
        const ratio = timeSubarea / timeUO;
        const passed = timeSubarea < 50 && ratio < 1.5;
        
        if (passed) {
            logTest('Performance de Consulta', 'PASS', 
                `Consulta por subárea: ${timeSubarea.toFixed(2)}ms (${ratio.toFixed(2)}x mais lenta que UO)`,
                { 
                    timeUO: `${timeUO.toFixed(2)}ms`, 
                    timeSubarea: `${timeSubarea.toFixed(2)}ms`,
                    recordsUO: allUO.length,
                    recordsSubarea: filtered.length
                });
        } else {
            logTest('Performance de Consulta', 'FAIL', 
                `Consulta muito lenta: ${timeSubarea.toFixed(2)}ms`,
                { 
                    timeUO: `${timeUO.toFixed(2)}ms`, 
                    timeSubarea: `${timeSubarea.toFixed(2)}ms`,
                    ratio: `${ratio.toFixed(2)}x`
                });
        }
        
        return passed;
        
    } catch (error) {
        logTest('Performance de Consulta', 'FAIL', error.message);
        return false;
    } finally {
        console.groupEnd();
    }
}

// ============================================================================
// TESTE 7: DADOS PRESERVADOS APÓS MIGRAÇÃO
// ============================================================================

async function test7_DataPreserved() {
    console.group('📋 TESTE 7: Dados Preservados Após Migração');
    
    try {
        const allRecords = await dbInterop.getAll('patrimonio');
        
        if (allRecords.length > 0) {
            logTest('Dados Preservados', 'PASS', 
                `${allRecords.length} registros encontrados no IndexedDB`,
                { sample: allRecords[0] });
        } else {
            logTest('Dados Preservados', 'FAIL', 
                'Nenhum registro encontrado (possível perda de dados)');
        }
        
        return allRecords.length > 0;
        
    } catch (error) {
        logTest('Dados Preservados', 'FAIL', error.message);
        return false;
    } finally {
        console.groupEnd();
    }
}

// ============================================================================
// TESTE 8: FILTROS OPCIONAIS
// ============================================================================

async function test8_OptionalFilters() {
    console.group('📋 TESTE 8: Filtros Opcionais');
    
    try {
        const { testUO, testArea } = TEST_CONFIG;
        
        // Apenas área (subárea null)
        const areaOnly = await dbInterop.getPatrimonioBySubarea(testUO, testArea, null);
        const allMatchArea = areaOnly.every(item => 
            item.cdUnid === testUO && item.cdArea === testArea
        );
        
        if (allMatchArea) {
            logTest('Filtros Opcionais', 'PASS', 
                `Filtro apenas por área funciona: ${areaOnly.length} bens`,
                { area: testArea });
        } else {
            logTest('Filtros Opcionais', 'FAIL', 
                'Filtro por área não está funcionando corretamente');
        }
        
        return allMatchArea;
        
    } catch (error) {
        logTest('Filtros Opcionais', 'FAIL', error.message);
        return false;
    } finally {
        console.groupEnd();
    }
}

// ============================================================================
// EXECUTAR TODOS OS TESTES
// ============================================================================

async function runAllTests() {
    console.clear();
    console.log('🚀 INICIANDO SUITE DE TESTES - Correção de Carregamento por Subárea');
    console.log('═'.repeat(80));
    console.log('');
    
    const results = {
        test1: await test1_VerifyIndexes(),
        test2: await test2_VerifyDBVersion(),
        test3: await test3_GetPatrimonioBySubarea(),
        test4: await test4_LegacyCompatibility(),
        test5: await test5_CodeNormalization(),
        test6: await test6_QueryPerformance(),
        test7: await test7_DataPreserved(),
        test8: await test8_OptionalFilters()
    };
    
    console.log('');
    console.log('═'.repeat(80));
    console.log('📊 RESUMO DOS TESTES');
    console.log('═'.repeat(80));
    
    const passed = Object.values(results).filter(r => r === true).length;
    const total = Object.keys(results).length;
    const percentage = ((passed / total) * 100).toFixed(1);
    
    console.log(`Total de Testes: ${total}`);
    console.log(`✅ Passou: ${passed}`);
    console.log(`❌ Falhou: ${total - passed}`);
    console.log(`📈 Taxa de Sucesso: ${percentage}%`);
    console.log('');
    
    if (passed === total) {
        console.log('🎉 TODOS OS TESTES PASSARAM! 🎉');
    } else {
        console.log('⚠️ ALGUNS TESTES FALHARAM - Revisar implementação');
    }
    
    console.log('═'.repeat(80));
    
    return results;
}

// ============================================================================
// TESTES INDIVIDUAIS (podem ser executados separadamente)
// ============================================================================

console.log('✅ Script de testes carregado!');
console.log('');
console.log('Para executar todos os testes:');
console.log('  await runAllTests()');
console.log('');
console.log('Para executar testes individuais:');
console.log('  await test1_VerifyIndexes()');
console.log('  await test2_VerifyDBVersion()');
console.log('  await test3_GetPatrimonioBySubarea()');
console.log('  await test4_LegacyCompatibility()');
console.log('  await test5_CodeNormalization()');
console.log('  await test6_QueryPerformance()');
console.log('  await test7_DataPreserved()');
console.log('  await test8_OptionalFilters()');
console.log('');
