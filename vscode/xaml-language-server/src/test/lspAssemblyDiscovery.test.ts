import * as assert from 'assert';
import * as fs from 'fs';
import * as os from 'os';
import * as path from 'path';
import * as lsp from '../lspAssemblyDiscovery';

// ---------------------------------------------------------------------------
// Test helpers
// ---------------------------------------------------------------------------

interface TempScope {
    dir: string;
}

/**
 * Create a temporary directory for testing and return a handle that can be
 * used to clean it up afterwards.
 */
function createTempScope(): TempScope {
    const dir = fs.mkdtempSync(path.join(os.tmpdir(), 'lsp-discovery-test-'));
    return { dir };
}

/**
 * Recursively remove a temporary directory.
 */
function disposeTempScope(scope: TempScope): void {
    fs.rmSync(scope.dir, { recursive: true, force: true });
}

/**
 * Create a directory hierarchy for a mock Windows SDK.
 *
 * Returns the full path to SDK root.
 */
function createMockSdk(tempRoot: string, version: string): string {
    const sdkRoot = path.join(tempRoot, 'Windows Kits', '10');
    fs.mkdirSync(path.join(sdkRoot, 'Platforms', 'UAP', version), { recursive: true });
    fs.mkdirSync(path.join(sdkRoot, 'References', version), { recursive: true });
    return sdkRoot;
}

/**
 * Create a simple .winmd file in the specified directory.
 */
function createWinmdFile(dir: string, name: string): string {
    const filePath = path.join(dir, name);
    fs.writeFileSync(filePath, '');
    return filePath;
}

// ---------------------------------------------------------------------------
// Suite: discoverWinSdkRoot
// ---------------------------------------------------------------------------

suite('discoverWinSdkRoot', () => {
    test('reads from env variable when set', () => {
        const originalValue = process.env.WindowsSdkDir;
        const testPath = 'C:\\Custom\\SDK\\10';

        process.env.WindowsSdkDir = testPath;
        try {
            const result = lsp.discoverWinSdkRoot();
            assert.strictEqual(result, testPath);
        } finally {
            process.env.WindowsSdkDir = originalValue;
        }
    });

    test('reads from registry when env not set', () => {
        const originalEnv = process.env.WindowsSdkDir;
        delete process.env.WindowsSdkDir;

        const cp: { execSync: (...args: unknown[]) => Buffer } = require('child_process');
        const originalExecSync = cp.execSync;

        cp.execSync = (..._args: unknown[]): Buffer => {
            return Buffer.from(
                '    KitsRoot10    REG_SZ    C:\\Program Files (x86)\\Windows Kits\\10\\',
            );
        };

        try {
            const result = lsp.discoverWinSdkRoot();
            assert.strictEqual(
                result,
                'C:\\Program Files (x86)\\Windows Kits\\10',
            );
        } finally {
            cp.execSync = originalExecSync;
            process.env.WindowsSdkDir = originalEnv;
        }
    });

    test('returns fallback when env not set and registry query fails', () => {
        const originalEnv = process.env.WindowsSdkDir;
        delete process.env.WindowsSdkDir;

        const cp: { execSync: (...args: unknown[]) => Buffer } = require('child_process');
        const originalExecSync = cp.execSync;

        cp.execSync = (): Buffer => {
            throw new Error('command not found');
        };

        try {
            const result = lsp.discoverWinSdkRoot();
            assert.ok(result.includes('Windows Kits'));
            assert.ok(result.includes('10'));
        } finally {
            cp.execSync = originalExecSync;
            process.env.WindowsSdkDir = originalEnv;
        }
    });
});

// ---------------------------------------------------------------------------
// Suite: discoverWinSdkVersion
// ---------------------------------------------------------------------------

suite('discoverWinSdkVersion', () => {
    test('returns the highest version directory', () => {
        const scope = createTempScope();
        try {
            const sdkRoot = path.join(scope.dir, 'SDK');
            const uapDir = path.join(sdkRoot, 'Platforms', 'UAP');

            fs.mkdirSync(path.join(uapDir, '10.0.17763.0'), { recursive: true });
            fs.mkdirSync(path.join(uapDir, '10.0.19041.0'), { recursive: true });
            fs.mkdirSync(path.join(uapDir, '10.0.22621.0'), { recursive: true });

            const result = lsp.discoverWinSdkVersion(sdkRoot);
            assert.strictEqual(result, '10.0.22621.0');
        } finally {
            disposeTempScope(scope);
        }
    });

    test('falls back to default when Platforms/UAP directory is missing', () => {
        const scope = createTempScope();
        try {
            const sdkRoot = path.join(scope.dir, 'EmptySDK');
            const result = lsp.discoverWinSdkVersion(sdkRoot);
            assert.strictEqual(result, '10.0.17763.0');
        } finally {
            disposeTempScope(scope);
        }
    });
});

// ---------------------------------------------------------------------------
// Suite: collectPlatformWinmds
// ---------------------------------------------------------------------------

suite('collectPlatformWinmds', () => {
    test('recursively collects all .winmd files under References/{version}', () => {
        const scope = createTempScope();
        try {
            const sdkRoot = path.join(scope.dir, 'SDK');
            const refDir = path.join(sdkRoot, 'References', '10.0.22621.0');

            // Create a nested reference directory structure similar to the real SDK
            fs.mkdirSync(path.join(refDir, 'Windows.Foundation'), { recursive: true });
            fs.mkdirSync(path.join(refDir, 'Windows.Foundation', 'Localization'), {
                recursive: true,
            });

            const winmd1 = createWinmdFile(refDir, 'Windows.Foundation.winmd');
            const winmd2 = createWinmdFile(
                path.join(refDir, 'Windows.Foundation'),
                'Windows.Foundation.FoundationContract.winmd',
            );
            const winmd3 = createWinmdFile(
                path.join(refDir, 'Windows.Foundation', 'Localization'),
                'Windows.Foundation.Localization.winmd',
            );
            // Non-winmd file that should be excluded
            createWinmdFile(refDir, 'readme.txt');

            const result = lsp.collectPlatformWinmds(sdkRoot, '10.0.22621.0');

            assert.strictEqual(result.length, 3);
            assert.ok(result.includes(winmd1));
            assert.ok(result.includes(winmd2));
            assert.ok(result.includes(winmd3));
        } finally {
            disposeTempScope(scope);
        }
    });

    test('returns empty array when references directory is missing', () => {
        const result = lsp.collectPlatformWinmds('C:\\Nonexistent\\SDK', '10.0.99999.0');
        assert.deepStrictEqual(result, []);
    });
});

// ---------------------------------------------------------------------------
// Suite: parseXmakeLuaVersions
// ---------------------------------------------------------------------------

suite('parseXmakeLuaVersions', () => {
    test('extracts all NuGet versions from xmake.lua', () => {
        const scope = createTempScope();
        try {
            const luaContent = [
                'add_rules("mode.debug", "mode.release")',
                'add_requires("Microsoft.WindowsAppSDK 1.6.0")',
                'add_requires("Microsoft.Web.WebView2 1.0.2535.41")',
                '',
                'target("MyApp")',
                '    set_kind("windowsapp")',
                '    add_files("src/*.cpp")',
            ].join('\n');

            const luaPath = path.join(scope.dir, 'xmake.lua');
            fs.writeFileSync(luaPath, luaContent, 'utf-8');

            const result = lsp.parseXmakeLuaVersions(luaPath);
            assert.strictEqual(result.winAppSdk, '1.6.0');
            assert.strictEqual(result.webView2, '1.0.2535.41');
        } finally {
            disposeTempScope(scope);
        }
    });

    test('falls back to packages.config when xmake.lua has no version info', () => {
        const scope = createTempScope();
        try {
            const luaContent = [
                'add_rules("mode.debug", "mode.release")',
                '',
                'target("MyApp")',
                '    set_kind("windowsapp")',
            ].join('\n');

            const luaPath = path.join(scope.dir, 'xmake.lua');
            fs.writeFileSync(luaPath, luaContent, 'utf-8');

            const configContent =
                '<?xml version="1.0" encoding="utf-8"?>\n' +
                '<packages>\n' +
                '  <package id="Microsoft.WindowsAppSDK" version="1.8.260416003" targetFramework="native" />\n' +
                '  <package id="Microsoft.Web.WebView2" version="1.0.3912.50" targetFramework="native" />\n' +
                '</packages>\n';
            fs.writeFileSync(path.join(scope.dir, 'packages.config'), configContent, 'utf-8');

            const result = lsp.parseXmakeLuaVersions(luaPath, scope.dir);
            assert.strictEqual(result.winAppSdk, '1.8.260416003');
            assert.strictEqual(result.webView2, '1.0.3912.50');
        } finally {
            disposeTempScope(scope);
        }
    });

    test('falls back to defaults when xmake.lua does not exist', () => {
        const result = lsp.parseXmakeLuaVersions('C:\\Nonexistent\\path\\xmake.lua');
        assert.strictEqual(result.winAppSdk, '1.6.0');
        assert.strictEqual(result.webView2, '1.0.2535.41');
    });
});

// ---------------------------------------------------------------------------
// Suite: parsePackagesConfigVersions
// ---------------------------------------------------------------------------

suite('parsePackagesConfigVersions', () => {
    test('extracts all versions from packages.config', () => {
        const scope = createTempScope();
        try {
            const configContent =
                '<?xml version="1.0" encoding="utf-8"?>\n' +
                '<packages>\n' +
                '  <package id="Microsoft.Windows.CppWinRT" version="2.0.250303.1" targetFramework="native" />\n' +
                '  <package id="Microsoft.WindowsAppSDK" version="1.8.260416003" targetFramework="native" />\n' +
                '  <package id="Microsoft.WindowsAppSDK.Foundation" version="1.8.260415000" targetFramework="native" />\n' +
                '  <package id="Microsoft.WindowsAppSDK.WinUI" version="1.8.260415005" targetFramework="native" />\n' +
                '  <package id="Microsoft.WindowsAppSDK.InteractiveExperiences" version="1.8.260415001" targetFramework="native" />\n' +
                '  <package id="Microsoft.Web.WebView2" version="1.0.3912.50" targetFramework="native" />\n' +
                '</packages>\n';

            const configPath = path.join(scope.dir, 'packages.config');
            fs.writeFileSync(configPath, configContent, 'utf-8');

            const result = lsp.parsePackagesConfigVersions(configPath);
            assert.ok(result !== null);
            assert.strictEqual(result!.winAppSdk, '1.8.260416003');
            assert.strictEqual(result!.webView2, '1.0.3912.50');
            assert.strictEqual(result!.foundation, '1.8.260415000');
            assert.strictEqual(result!.winui, '1.8.260415005');
            assert.strictEqual(result!.interactive, '1.8.260415001');
        } finally {
            disposeTempScope(scope);
        }
    });

    test('returns null when Microsoft.WindowsAppSDK is missing', () => {
        const scope = createTempScope();
        try {
            const configContent =
                '<?xml version="1.0" encoding="utf-8"?>\n' +
                '<packages>\n' +
                '  <package id="Microsoft.Web.WebView2" version="1.0.3912.50" targetFramework="native" />\n' +
                '</packages>\n';

            const configPath = path.join(scope.dir, 'packages.config');
            fs.writeFileSync(configPath, configContent, 'utf-8');

            const result = lsp.parsePackagesConfigVersions(configPath);
            assert.strictEqual(result, null);
        } finally {
            disposeTempScope(scope);
        }
    });

    test('returns null when file does not exist', () => {
        const result = lsp.parsePackagesConfigVersions('C:\\Nonexistent\\packages.config');
        assert.strictEqual(result, null);
    });

    test('uses umbrella version for sub-packages when absent', () => {
        const scope = createTempScope();
        try {
            const configContent =
                '<?xml version="1.0" encoding="utf-8"?>\n' +
                '<packages>\n' +
                '  <package id="Microsoft.WindowsAppSDK" version="1.7.2" targetFramework="native" />\n' +
                '</packages>\n';

            const configPath = path.join(scope.dir, 'packages.config');
            fs.writeFileSync(configPath, configContent, 'utf-8');

            const result = lsp.parsePackagesConfigVersions(configPath);
            assert.ok(result !== null);
            assert.strictEqual(result!.foundation, '1.7.2');
            assert.strictEqual(result!.winui, '1.7.2');
            assert.strictEqual(result!.interactive, '1.7.2');
        } finally {
            disposeTempScope(scope);
        }
    });
});

// ---------------------------------------------------------------------------
// Suite: extractNamespace
// ---------------------------------------------------------------------------

suite('extractNamespace', () => {
    test('extracts from demo xmake.lua with set_values', () => {
        const scope = createTempScope();
        try {
            // Create demo/hello/xmake.lua structure
            const demoDir = path.join(scope.dir, 'demo');
            fs.mkdirSync(demoDir, { recursive: true });
            const helloDir = path.join(demoDir, 'hello');
            fs.mkdirSync(helloDir, { recursive: true });

            const luaContent = [
                'target("demo.hello")',
                '    add_rules("winui3.app")',
                '    set_values("winui3.namespace", "hello")',
                '    add_files("src/*.cpp")',
            ].join('\n');
            fs.writeFileSync(path.join(helloDir, 'xmake.lua'), luaContent, 'utf-8');

            const result = lsp.extractNamespace(scope.dir);
            assert.strictEqual(result, 'hello');
        } finally {
            disposeTempScope(scope);
        }
    });

    test('extracts from XAML x:Class when no xmake.lua', () => {
        const scope = createTempScope();
        try {
            const srcDir = path.join(scope.dir, 'src');
            fs.mkdirSync(srcDir, { recursive: true });
            fs.writeFileSync(
                path.join(srcDir, 'App.xaml'),
                '<Application x:Class="myapp.App" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" />',
                'utf-8',
            );

            const result = lsp.extractNamespace(scope.dir);
            assert.strictEqual(result, 'myapp');
        } finally {
            disposeTempScope(scope);
        }
    });

    test('falls back to workspace basename', () => {
        const scope = createTempScope();
        try {
            const result = lsp.extractNamespace(scope.dir);
            assert.strictEqual(result, path.basename(scope.dir));
        } finally {
            disposeTempScope(scope);
        }
    });
});

// ---------------------------------------------------------------------------
// Suite: resolveSourceDir (recursive)
// ---------------------------------------------------------------------------

suite('resolveSourceDir', () => {
    test('returns src/ subdirectory when it exists', () => {
        const scope = createTempScope();
        try {
            fs.mkdirSync(path.join(scope.dir, 'src'), { recursive: true });
            fs.writeFileSync(path.join(scope.dir, 'src', 'App.xaml'), '');
            const result = lsp.resolveSourceDir(scope.dir);
            assert.strictEqual(result, path.join(scope.dir, 'src'));
        } finally {
            disposeTempScope(scope);
        }
    });

    test('finds nested demo/hello/src with .xaml files', () => {
        const scope = createTempScope();
        try {
            // Simulate winui3_xmake project structure
            const helloDir = path.join(scope.dir, 'demo', 'hello');
            const srcDir = path.join(helloDir, 'src');
            fs.mkdirSync(srcDir, { recursive: true });
            fs.writeFileSync(path.join(srcDir, 'App.xaml'), '');
            fs.writeFileSync(path.join(srcDir, 'MainWindow.xaml'), '');

            const result = lsp.resolveSourceDir(scope.dir);
            assert.ok(result.includes('src'), `Expected ${result} to contain 'src'`);
        } finally {
            disposeTempScope(scope);
        }
    });

    test('returns workspace root when no XAML files exist anywhere', () => {
        const scope = createTempScope();
        try {
            fs.mkdirSync(path.join(scope.dir, 'empty'), { recursive: true });
            const result = lsp.resolveSourceDir(scope.dir);
            assert.strictEqual(result, scope.dir);
        } finally {
            disposeTempScope(scope);
        }
    });
});

// ---------------------------------------------------------------------------
// Suite: findXamlSrcDir (recursive)
// ---------------------------------------------------------------------------

suite('findXamlSrcDir', () => {
    test('finds the first child directory containing .xaml files', () => {
        const scope = createTempScope();
        try {
            fs.mkdirSync(path.join(scope.dir, 'views'), { recursive: true });
            fs.writeFileSync(path.join(scope.dir, 'views', 'MainPage.xaml'), '');

            const result = lsp.findXamlSrcDir(scope.dir);
            assert.strictEqual(result, path.join(scope.dir, 'views'));
        } finally {
            disposeTempScope(scope);
        }
    });

    test('finds deeply nested source directory', () => {
        const scope = createTempScope();
        try {
            const nestedDir = path.join(scope.dir, 'projects', 'demo1', 'src');
            fs.mkdirSync(nestedDir, { recursive: true });
            fs.writeFileSync(path.join(nestedDir, 'Page.xaml'), '');

            const result = lsp.findXamlSrcDir(scope.dir);
            assert.ok(result !== null);
            assert.ok(result!.includes('src'), `Expected ${result} to contain 'src'`);
        } finally {
            disposeTempScope(scope);
        }
    });

    test('skips build/ node_modules/ .git/ directories', () => {
        const scope = createTempScope();
        try {
            // Create build/ with .xaml files (should be ignored)
            fs.mkdirSync(path.join(scope.dir, 'build', 'generated'), { recursive: true });
            fs.writeFileSync(path.join(scope.dir, 'build', 'generated', 'App.xaml'), '');

            // Doesn't have real .xaml in source directories
            const result = lsp.findXamlSrcDir(scope.dir);
            assert.strictEqual(result, null);
        } finally {
            disposeTempScope(scope);
        }
    });

    test('returns null when no directory contains .xaml files', () => {
        const scope = createTempScope();
        try {
            fs.mkdirSync(path.join(scope.dir, 'emptyDir'), { recursive: true });
            const result = lsp.findXamlSrcDir(scope.dir);
            assert.strictEqual(result, null);
        } finally {
            disposeTempScope(scope);
        }
    });
});

// ---------------------------------------------------------------------------
// Suite: discoverBestNuGetVersions
// ---------------------------------------------------------------------------

suite('discoverBestNuGetVersions', () => {
    test('returns highest installed version from NuGet cache', () => {
        const scope = createTempScope();
        try {
            // Create mock NuGet cache with multiple versions
            const packagesDir = path.join(scope.dir, 'packages');
            fs.mkdirSync(packagesDir, { recursive: true });

            fs.mkdirSync(path.join(packagesDir, 'microsoft.windowsappsdk.1.7.2'));
            fs.mkdirSync(path.join(packagesDir, 'microsoft.windowsappsdk.1.8.260416003'));
            fs.mkdirSync(path.join(packagesDir, 'microsoft.windowsappsdk.foundation.1.8.260415000'));
            fs.mkdirSync(path.join(packagesDir, 'microsoft.web.webview2.1.0.2535.41'));
            fs.mkdirSync(path.join(packagesDir, 'microsoft.web.webview2.1.0.3912.50'));

            const result = lsp.discoverBestNuGetVersions(packagesDir);
            assert.strictEqual(result.winAppSdk, '1.8.260416003');
            assert.strictEqual(result.foundation, '1.8.260415000');
            assert.strictEqual(result.webView2, '1.0.3912.50');
        } finally {
            disposeTempScope(scope);
        }
    });

    test('returns updated defaults when NuGet cache is empty', () => {
        const scope = createTempScope();
        try {
            const packagesDir = path.join(scope.dir, 'packages');
            fs.mkdirSync(packagesDir, { recursive: true });

            const result = lsp.discoverBestNuGetVersions(packagesDir);
            assert.ok(result.winAppSdk.startsWith('1.8.'));
            assert.ok(result.webView2.startsWith('1.0.'));
        } finally {
            disposeTempScope(scope);
        }
    });
});

// ---------------------------------------------------------------------------
// Suite: collectSubPackageAppSdkWinmds
// ---------------------------------------------------------------------------

suite('collectSubPackageAppSdkWinmds', () => {
    test('collects WinMDs from foundation sub-package', () => {
        const scope = createTempScope();
        try {
            const nugetRoot = path.join(scope.dir, 'packages');
            const metadataDir = path.join(
                nugetRoot,
                'microsoft.windowsappsdk.foundation',
                '1.8.260415000',
                'metadata',
            );
            fs.mkdirSync(metadataDir, { recursive: true });
            createWinmdFile(metadataDir, 'Microsoft.Windows.Foundation.winmd');
            createWinmdFile(metadataDir, 'Microsoft.Windows.AppLifecycle.winmd');

            const versions: lsp.NuGetVersions = {
                winAppSdk: '1.8.260416003',
                webView2: '1.0.3912.50',
                foundation: '1.8.260415000',
                winui: '1.8.260415005',
                interactive: '1.8.260415001',
            };

            const result = lsp.collectSubPackageAppSdkWinmds(nugetRoot, versions);
            assert.strictEqual(result.length, 2);
        } finally {
            disposeTempScope(scope);
        }
    });

    test('collects WinMDs from multiple sub-packages', () => {
        const scope = createTempScope();
        try {
            const nugetRoot = path.join(scope.dir, 'packages');

            // Foundation
            const foundationMd = path.join(nugetRoot, 'microsoft.windowsappsdk.foundation', '1.8.260415000', 'metadata');
            fs.mkdirSync(foundationMd, { recursive: true });
            createWinmdFile(foundationMd, 'Microsoft.Windows.Foundation.winmd');

            // WinUI
            const winuiMd = path.join(nugetRoot, 'microsoft.windowsappsdk.winui', '1.8.260415005', 'metadata');
            fs.mkdirSync(winuiMd, { recursive: true });
            createWinmdFile(winuiMd, 'Microsoft.UI.Xaml.winmd');

            // IXP (nested)
            const ixpMd = path.join(nugetRoot, 'microsoft.windowsappsdk.interactiveexperiences', '1.8.260415001', 'metadata', '10.0.18362.0');
            fs.mkdirSync(ixpMd, { recursive: true });
            createWinmdFile(ixpMd, 'Microsoft.UI.winmd');

            const versions: lsp.NuGetVersions = {
                winAppSdk: '1.8.260416003',
                webView2: '1.0.3912.50',
                foundation: '1.8.260415000',
                winui: '1.8.260415005',
                interactive: '1.8.260415001',
            };

            const result = lsp.collectSubPackageAppSdkWinmds(nugetRoot, versions);
            assert.strictEqual(result.length, 3);
        } finally {
            disposeTempScope(scope);
        }
    });

    test('falls back to monolithic package when sub-packages absent', () => {
        const scope = createTempScope();
        try {
            const nugetRoot = path.join(scope.dir, 'packages');
            const pkgDir = path.join(
                nugetRoot,
                'microsoft.windowsappsdk',
                '1.6.0',
                'lib',
                'net6.0-windows10.0.19041.0',
            );
            fs.mkdirSync(pkgDir, { recursive: true });
            createWinmdFile(pkgDir, 'Microsoft.WindowsAppSDK.winmd');

            const versions: lsp.NuGetVersions = {
                winAppSdk: '1.6.0',
                webView2: '1.0.2535.41',
                foundation: '1.6.0',
                winui: '1.6.0',
                interactive: '1.6.0',
            };

            const result = lsp.collectSubPackageAppSdkWinmds(nugetRoot, versions);
            assert.strictEqual(result.length, 1);
        } finally {
            disposeTempScope(scope);
        }
    });
});

// ---------------------------------------------------------------------------
// Suite: collectAppSdkWinmds
// ---------------------------------------------------------------------------

suite('collectAppSdkWinmds', () => {
    test('finds WinMDs in NuGet package cache', () => {
        const scope = createTempScope();
        try {
            const nugetRoot = path.join(scope.dir, '.nuget', 'packages');
            const pkgDir = path.join(
                nugetRoot,
                'microsoft.windowsappsdk',
                '1.6.0',
                'lib',
                'net6.0-windows10.0.19041.0',
            );
            fs.mkdirSync(pkgDir, { recursive: true });

            const winmdFile = createWinmdFile(pkgDir, 'Microsoft.WindowsAppSDK.winmd');

            const versions: lsp.XmakeVersions = {
                winAppSdk: '1.6.0',
                webView2: '1.0.2535.41',
            };

            const result = lsp.collectAppSdkWinmds(nugetRoot, versions);
            assert.strictEqual(result.length, 1);
            assert.strictEqual(result[0], winmdFile);
        } finally {
            disposeTempScope(scope);
        }
    });

    test('returns empty array when NuGet package is missing', () => {
        const versions: lsp.XmakeVersions = {
            winAppSdk: '99.99.99',
            webView2: '1.0.2535.41',
        };
        const result = lsp.collectAppSdkWinmds('C:\\Nonexistent\\NuGet', versions);
        assert.deepStrictEqual(result, []);
    });
});

// ---------------------------------------------------------------------------
// Suite: discoverWebView2Winmd
// ---------------------------------------------------------------------------

suite('discoverWebView2Winmd', () => {
    test('finds WebView2 WinMD in TFM subdirectory', () => {
        const scope = createTempScope();
        try {
            const nugetRoot = path.join(scope.dir, '.nuget', 'packages');
            const pkgDir = path.join(
                nugetRoot,
                'microsoft.web.webview2',
                '1.0.2535.41',
                'lib',
                'net6.0-windows10.0.19041.0',
            );
            fs.mkdirSync(pkgDir, { recursive: true });
            const winmdFile = createWinmdFile(pkgDir, 'Microsoft.Web.WebView2.Core.winmd');

            const result = lsp.discoverWebView2Winmd(nugetRoot, '1.0.2535.41');
            assert.strictEqual(result, winmdFile);
        } finally {
            disposeTempScope(scope);
        }
    });

    test('finds WebView2 WinMD directly in lib/ (flat layout)', () => {
        const scope = createTempScope();
        try {
            const nugetRoot = path.join(scope.dir, '.nuget', 'packages');
            const libDir = path.join(
                nugetRoot,
                'microsoft.web.webview2',
                '1.0.3912.50',
                'lib',
            );
            fs.mkdirSync(libDir, { recursive: true });
            const winmdFile = createWinmdFile(libDir, 'Microsoft.Web.WebView2.Core.winmd');

            // Also create a TFM subdirectory to ensure flat layout is preferred
            fs.mkdirSync(path.join(libDir, 'net6.0-windows10.0.19041.0'), { recursive: true });

            const result = lsp.discoverWebView2Winmd(nugetRoot, '1.0.3912.50');
            assert.strictEqual(result, winmdFile);
        } finally {
            disposeTempScope(scope);
        }
    });

    test('returns null when WebView2 package is missing', () => {
        const result = lsp.discoverWebView2Winmd('C:\\Nonexistent\\NuGet', '0.0.0');
        assert.strictEqual(result, null);
    });
});

// ---------------------------------------------------------------------------
// Suite: discoverLspContext (integration)
// ---------------------------------------------------------------------------

suite('discoverLspContext', () => {
    test('returns a valid context when environment is configured', async () => {
        const scope = createTempScope();
        try {
            // Set up a minimal workspace root
            const workspaceRoot = scope.dir;
            fs.mkdirSync(path.join(workspaceRoot, 'src'), { recursive: true });

            // Create xmake.lua
            const luaContent =
                'add_requires("Microsoft.WindowsAppSDK 1.6.0")\n' +
                'add_requires("Microsoft.Web.WebView2 1.0.2535.41")\n';
            fs.writeFileSync(path.join(workspaceRoot, 'xmake.lua'), luaContent, 'utf-8');

            // Set up a mock Windows SDK isolated to a temp directory
            const mockSdkVersion = '10.0.22621.0';
            const mockSdkRoot = createMockSdk(scope.dir, mockSdkVersion);

            // Create some platform winmd files
            createWinmdFile(
                path.join(mockSdkRoot, 'References', mockSdkVersion),
                'Windows.Foundation.winmd',
            );

            // Set environment so discoverWinSdkRoot returns our mock SDK
            const originalSdkDir = process.env.WindowsSdkDir;
            process.env.WindowsSdkDir = mockSdkRoot;

            // Set up a mock NuGet cache
            const nugetRoot = path.join(scope.dir, 'NuGetCache', '.nuget', 'packages');
            const userProfileEnv = process.env.USERPROFILE;
            process.env.USERPROFILE = path.join(scope.dir, 'NuGetCache');

            // Create WinAppSDK winmd
            const appSdkDir = path.join(
                nugetRoot,
                'microsoft.windowsappsdk',
                '1.6.0',
                'lib',
                'net6.0-windows10.0.19041.0',
            );
            fs.mkdirSync(appSdkDir, { recursive: true });
            createWinmdFile(appSdkDir, 'Microsoft.WindowsAppSDK.winmd');

            // Create WebView2 winmd
            const webView2Dir = path.join(
                nugetRoot,
                'microsoft.web.webview2',
                '1.0.2535.41',
                'lib',
                'net6.0-windows10.0.19041.0',
            );
            fs.mkdirSync(webView2Dir, { recursive: true });
            createWinmdFile(webView2Dir, 'Microsoft.Web.WebView2.Core.winmd');

            try {
                const result = await lsp.discoverLspContext(workspaceRoot);

                assert.ok(result !== null, 'Context should not be null');

                // SDK root
                assert.strictEqual(result!.winSdkRoot, mockSdkRoot);

                // SDK version should be the highest found in our mock Platforms/UAP
                assert.strictEqual(result!.winSdkVersion, mockSdkVersion);

                // namespace derived from workspace root basename
                assert.strictEqual(result!.namespace, path.basename(workspaceRoot));

                // source directory resolved to src/
                assert.strictEqual(result!.sourceDir, path.join(workspaceRoot, 'src'));

                // reference assemblies: platform winmd + app sdk + webview2 = 3
                assert.strictEqual(result!.referenceAssemblies.length, 3);
            } finally {
                process.env.WindowsSdkDir = originalSdkDir;
                process.env.USERPROFILE = userProfileEnv;
            }
        } finally {
            disposeTempScope(scope);
        }
    });

    test('returns null when an unexpected error occurs', async () => {
        // Trigger an error by passing a non-string value that causes
        // path.basename() to throw a TypeError caught by the outer catch.
        const result = await lsp.discoverLspContext(
            undefined as unknown as string,
        );
        assert.strictEqual(result, null);
    });

    test('discovers WinUI context from winui3_xmake project structure', async () => {
        const scope = createTempScope();
        try {
            const workspaceRoot = scope.dir;

            // Create winui3_xmake-style directory layout (no src/ at root)
            const srcDir = path.join(workspaceRoot, 'demo', 'hello', 'src');
            fs.mkdirSync(srcDir, { recursive: true });
            fs.writeFileSync(path.join(srcDir, 'App.xaml'), '<Application x:Class="hello.App" xmlns="..." />');
            fs.writeFileSync(path.join(srcDir, 'MainWindow.xaml'), '<Window x:Class="hello.MainWindow" xmlns="..." />');

            // Create demo-level xmake.lua with namespace
            const demoDir = path.join(workspaceRoot, 'demo', 'hello');
            fs.writeFileSync(
                path.join(demoDir, 'xmake.lua'),
                'target("demo.hello")\n' +
                '    set_values("winui3.namespace", "hello")\n',
                'utf-8',
            );

            // Create root xmake.lua (no add_requires — uses includes instead)
            fs.writeFileSync(
                path.join(workspaceRoot, 'xmake.lua'),
                'set_project("winui3_demos")\n' +
                'includes("rules/winui3.lua")\n',
                'utf-8',
            );

            // Create packages.config with sub-package versions
            fs.writeFileSync(
                path.join(workspaceRoot, 'packages.config'),
                '<?xml version="1.0" encoding="utf-8"?>\n' +
                '<packages>\n' +
                '  <package id="Microsoft.WindowsAppSDK" version="1.8.260416003" targetFramework="native" />\n' +
                '  <package id="Microsoft.WindowsAppSDK.Foundation" version="1.8.260415000" targetFramework="native" />\n' +
                '  <package id="Microsoft.WindowsAppSDK.WinUI" version="1.8.260415005" targetFramework="native" />\n' +
                '  <package id="Microsoft.Web.WebView2" version="1.0.3912.50" targetFramework="native" />\n' +
                '</packages>\n',
                'utf-8',
            );

            // Set up mock Windows SDK
            const mockSdkVersion = '10.0.22621.0';
            const mockSdkRoot = createMockSdk(scope.dir, mockSdkVersion);
            createWinmdFile(
                path.join(mockSdkRoot, 'References', mockSdkVersion),
                'Windows.Foundation.winmd',
            );

            const originalSdkDir = process.env.WindowsSdkDir;
            process.env.WindowsSdkDir = mockSdkRoot;

            // Set up mock NuGet cache with sub-packages
            const nugetRoot = path.join(scope.dir, 'NuGetCache', '.nuget', 'packages');
            const userProfileEnv = process.env.USERPROFILE;
            process.env.USERPROFILE = path.join(scope.dir, 'NuGetCache');

            // Foundation sub-package
            const foundationMd = path.join(nugetRoot, 'microsoft.windowsappsdk.foundation', '1.8.260415000', 'metadata');
            fs.mkdirSync(foundationMd, { recursive: true });
            createWinmdFile(foundationMd, 'Microsoft.Windows.Foundation.winmd');

            // WinUI sub-package
            const winuiMd = path.join(nugetRoot, 'microsoft.windowsappsdk.winui', '1.8.260415005', 'metadata');
            fs.mkdirSync(winuiMd, { recursive: true });
            createWinmdFile(winuiMd, 'Microsoft.UI.Xaml.winmd');

            // WebView2 (flat layout)
            const wv2Dir = path.join(nugetRoot, 'microsoft.web.webview2', '1.0.3912.50', 'lib');
            fs.mkdirSync(wv2Dir, { recursive: true });
            createWinmdFile(wv2Dir, 'Microsoft.Web.WebView2.Core.winmd');

            try {
                const result = await lsp.discoverLspContext(workspaceRoot);

                assert.ok(result !== null, 'Context should not be null');
                assert.ok(result!.referenceAssemblies.length >= 3,
                    `Expected >= 3 assemblies, got ${result!.referenceAssemblies.length}`);
                // Namespace should be extracted from demo xmake.lua
                assert.strictEqual(result!.namespace, 'hello');
                // Source directory should contain .xaml files
                assert.ok(
                    result!.sourceDir.includes('src') || result!.sourceDir.includes('hello'),
                    `Expected sourceDir to contain 'src' or 'hello', got: ${result!.sourceDir}`,
                );
            } finally {
                process.env.WindowsSdkDir = originalSdkDir;
                process.env.USERPROFILE = userProfileEnv;
            }
        } finally {
            disposeTempScope(scope);
        }
    });
});
