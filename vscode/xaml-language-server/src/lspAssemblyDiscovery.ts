import * as fs from 'fs';
import * as path from 'path';
import { execSync } from 'child_process';
import { workspace } from 'vscode';

// ---------------------------------------------------------------------------
// Public types
// ---------------------------------------------------------------------------

export interface LspAssemblyContext {
    referenceAssemblies: string[];
    winSdkRoot: string;
    winSdkVersion: string;
    namespace: string;
    sourceDir: string;
}

/**
 * Result of a project WinMD discovery attempt.
 *
 * The {@code warnings} array carries human-readable diagnostics that explain
 * why discovery returned empty or ambiguous results. These are intended for
 * developer debugging (e.g. via the VS Code output channel), not for end-user
 * display.
 */
export interface DiscoverProjectWinmdsResult {
    /** Discovered project WinMD absolute paths */
    paths: string[];
    /** Human-readable warnings for debugging (not shown to end users) */
    warnings: string[];
}

export interface XmakeVersions {
    winAppSdk: string;
    webView2: string;
}

export interface NuGetVersions extends XmakeVersions {
    /** Microsoft.WindowsAppSDK.Foundation 子包版本 */
    foundation: string;
    /** Microsoft.WindowsAppSDK.WinUI 子包版本 */
    winui: string;
    /** Microsoft.WindowsAppSDK.InteractiveExperiences 子包版本 */
    interactive: string;
    /** Microsoft.Graphics.Win2D 包版本 */
    win2d: string;
}

// ---------------------------------------------------------------------------
// Windows SDK discovery
// ---------------------------------------------------------------------------

/**
 * Discover the Windows SDK installation root directory.
 *
 * Priority order:
 *   1. `WindowsSdkDir` environment variable
 *   2. Registry query via `reg query`
 *   3. Hardcoded fallback path
 */
export function discoverWinSdkRoot(): string {
    // Priority 1: environment variable
    const envSdkDir = process.env.WindowsSdkDir;
    if (envSdkDir) {
        return envSdkDir;
    }

    // Priority 2: registry query
    try {
        const output = execSync(
            'reg query "HKLM\\SOFTWARE\\Microsoft\\Windows Kits\\Installed Roots" /v KitsRoot10',
        ).toString();
        const match = output.match(/KitsRoot10\s+REG_SZ\s+(.+)/);
        if (match) {
            return match[1].trim().replace(/\\$/, '');
        }
    } catch {
        // Registry query failed; proceed to fallback
    }

    // Priority 3: hardcoded fallback
    return 'C:\\Program Files (x86)\\Windows Kits\\10';
}

/**
 * Compare two dot-separated version strings numerically (descending order).
 */
export function compareVersions(a: string, b: string): number {
    const partsA = a.split('.').map(Number);
    const partsB = b.split('.').map(Number);
    const len = Math.max(partsA.length, partsB.length);
    for (let i = 0; i < len; i++) {
        const va = partsA[i] ?? 0;
        const vb = partsB[i] ?? 0;
        if (va !== vb) {
            return va - vb;
        }
    }
    return 0;
}

/**
 * Determine the highest available Windows SDK version by inspecting
 * the Platforms/UAP/ directory.
 */
export function discoverWinSdkVersion(sdkRoot: string): string {
    const uapDir = path.join(sdkRoot, 'Platforms', 'UAP');
    if (!fs.existsSync(uapDir)) {
        return '10.0.17763.0';
    }

    try {
        const entries = fs.readdirSync(uapDir, { withFileTypes: true });
        const versions = entries
            .filter(e => e.isDirectory())
            .map(e => e.name)
            .filter(name => /^\d+\.\d+\.\d+\.\d+$/.test(name))
            .sort((a, b) => compareVersions(b, a)); // descending -> highest first

        return versions.length > 0 ? versions[0] : '10.0.17763.0';
    } catch {
        return '10.0.17763.0';
    }
}

// ---------------------------------------------------------------------------
// Platform WinMD collection
// ---------------------------------------------------------------------------

/**
 * Recursively collect all `.winmd` files from a given directory tree.
 */
function collectWinmdsRecursive(dir: string): string[] {
    const results: string[] = [];
    const entries = fs.readdirSync(dir, { withFileTypes: true });

    for (const entry of entries) {
        const fullPath = path.join(dir, entry.name);
        if (entry.isDirectory()) {
            results.push(...collectWinmdsRecursive(fullPath));
        } else if (entry.isFile() && entry.name.toLowerCase().endsWith('.winmd')) {
            results.push(fullPath);
        }
    }

    return results;
}

/**
 * Collect platform (SDK) WinMDs from a given SDK root and version.
 */
export function collectPlatformWinmds(sdkRoot: string, sdkVersion: string): string[] {
    const refDir = path.join(sdkRoot, 'References', sdkVersion);
    if (!fs.existsSync(refDir)) {
        return [];
    }

    try {
        return collectWinmdsRecursive(refDir);
    } catch {
        return [];
    }
}

// ---------------------------------------------------------------------------
// packages.config version parsing
// ---------------------------------------------------------------------------

/**
 * Parse NuGet package versions from a packages.config XML file.
 *
 * Expected format:
 *   <package id="Microsoft.WindowsAppSDK" version="1.8.260416003" ... />
 *   <package id="Microsoft.WindowsAppSDK.Foundation" version="1.8.260415000" ... />
 *   <package id="Microsoft.Web.WebView2" version="1.0.3912.50" ... />
 */
export function parsePackagesConfigVersions(
    configPath: string,
): NuGetVersions | null {
    try {
        const content = fs.readFileSync(configPath, 'utf-8');

        const umbrellaMatch = content.match(
            /id="Microsoft\.WindowsAppSDK"\s+version="([\d.]+)"/,
        );
        const foundationMatch = content.match(
            /id="Microsoft\.WindowsAppSDK\.Foundation"\s+version="([\d.]+)"/,
        );
        const winuiMatch = content.match(
            /id="Microsoft\.WindowsAppSDK\.WinUI"\s+version="([\d.]+)"/,
        );
        const interactiveMatch = content.match(
            /id="Microsoft\.WindowsAppSDK\.InteractiveExperiences"\s+version="([\d.]+)"/,
        );
        const webview2Match = content.match(
            /id="Microsoft\.Web\.WebView2"\s+version="([\d.]+)"/,
        );
        const win2dMatch = content.match(
            /id="Microsoft\.Graphics\.Win2D"\s+version="([\d.]+)"/,
        );

        if (!umbrellaMatch) {
            return null;
        }

        return {
            winAppSdk: umbrellaMatch[1],
            webView2: webview2Match ? webview2Match[1] : '1.0.3912.50',
            foundation: foundationMatch ? foundationMatch[1] : umbrellaMatch[1],
            winui: winuiMatch ? winuiMatch[1] : umbrellaMatch[1],
            interactive: interactiveMatch ? interactiveMatch[1] : umbrellaMatch[1],
            win2d: win2dMatch ? win2dMatch[1] : '1.4.0',
        };
    } catch {
        return null;
    }
}

// ---------------------------------------------------------------------------
// xmake.lua version parsing
// ---------------------------------------------------------------------------

/**
 * Parse NuGet package version information from an xmake.lua file.
 *
 * Expected line format:
 *   add_requires("Microsoft.WindowsAppSDK 1.6.0")
 *   add_requires("Microsoft.Web.WebView2 1.0.2535.41")
 *
 * When the xmake.lua does not contain version declarations, falls back to
 * parsing the sibling {@code packages.config} file if present.
 *
 * @param luaPath - Path to the xmake.lua file.
 * @param workspaceRoot - Optional workspace root; used to locate packages.config.
 */
export function parseXmakeLuaVersions(
    luaPath: string,
    workspaceRoot?: string,
): XmakeVersions {
    const defaults: XmakeVersions = {
        winAppSdk: '1.6.0',
        webView2: '1.0.2535.41',
    };

    try {
        const content = fs.readFileSync(luaPath, 'utf-8');
        let foundAppSdk = false;
        let foundWebView2 = false;
        const results: XmakeVersions = { ...defaults };

        const appSdkMatch = content.match(
            /add_requires\(["']Microsoft\.WindowsAppSDK\s+([\d.]+)["']\)/,
        );
        if (appSdkMatch) {
            results.winAppSdk = appSdkMatch[1];
            foundAppSdk = true;
        }

        const webView2Match = content.match(
            /add_requires\(["']Microsoft\.Web\.WebView2\s+([\d.]+)["']\)/,
        );
        if (webView2Match) {
            results.webView2 = webView2Match[1];
            foundWebView2 = true;
        }

        // If no versions were found in xmake.lua, try packages.config
        if (!foundAppSdk && !foundWebView2 && workspaceRoot) {
            const configPath = path.join(workspaceRoot, 'packages.config');
            if (fs.existsSync(configPath)) {
                try {
                    const pkgVersions = parsePackagesConfigVersions(configPath);
                    if (pkgVersions) {
                        results.winAppSdk = pkgVersions.winAppSdk;
                        results.webView2 = pkgVersions.webView2;
                    }
                } catch {
                    // packages.config fallback failed; keep defaults
                }
            }
        }

        return results;
    } catch {
        return defaults;
    }
}

// ---------------------------------------------------------------------------
// NuGet cache WinMD collection
// ---------------------------------------------------------------------------

/**
 * Collect WinAppSDK WinMDs from the NuGet package cache.
 */
export function collectAppSdkWinmds(nugetRoot: string, versions: XmakeVersions): string[] {
    const packageDir = path.join(
        nugetRoot,
        'microsoft.windowsappsdk',
        versions.winAppSdk,
        'lib',
    );

    if (!fs.existsSync(packageDir)) {
        return [];
    }

    try {
        const tfmDirs = fs
            .readdirSync(packageDir, { withFileTypes: true })
            .filter(e => e.isDirectory())
            .map(e => path.join(packageDir, e.name));

        const winmds: string[] = [];
        for (const tfmDir of tfmDirs) {
            const files = fs.readdirSync(tfmDir);
            for (const file of files) {
                if (file.toLowerCase().endsWith('.winmd')) {
                    winmds.push(path.join(tfmDir, file));
                }
            }
        }
        return winmds;
    } catch {
        return [];
    }
}

/**
 * Collect WinAppSDK WinMDs from sub-packages in the NuGet cache.
 *
 * WinAppSDK 1.6+ splits its WinMDs across three sub-packages, each
 * placing them under a {@code metadata/} directory:
 *
 *   - microsoft.windowsappsdk.foundation/{v}/metadata/   (flat)
 *   - microsoft.windowsappsdk.winui/{v}/metadata/         (flat)
 *   - microsoft.windowsappsdk.interactiveexperiences/{v}/metadata/ (versioned subdirs)
 *
 * Falls back to the monolithic package layout when sub-packages are absent.
 */
export function collectSubPackageAppSdkWinmds(
    nugetRoot: string,
    versions: NuGetVersions,
): string[] {
    const subPackageNames = [
        'microsoft.windowsappsdk.foundation',
        'microsoft.windowsappsdk.winui',
        'microsoft.windowsappsdk.interactiveexperiences',
    ];
    const subPackageVersions = [
        versions.foundation,
        versions.winui,
        versions.interactive,
    ];

    const winmds: string[] = [];

    for (let i = 0; i < subPackageNames.length; i++) {
        const metadataDir = path.join(
            nugetRoot,
            subPackageNames[i],
            subPackageVersions[i],
            'metadata',
        );
        if (fs.existsSync(metadataDir)) {
            try {
                const collected = collectWinmdsRecursive(metadataDir);
                winmds.push(...collected);
            } catch {
                // permission / access error; skip this sub-package
                continue;
            }
        }
    }

    // Fallback to monolithic package
    if (winmds.length === 0) {
        return collectAppSdkWinmds(nugetRoot, { winAppSdk: versions.winAppSdk, webView2: versions.webView2 });
    }

    return winmds;
}

/**
 * Find the WebView2 WinMD file from the NuGet package cache.
 */
export function discoverWebView2Winmd(nugetRoot: string, version: string): string | null {
    const packageDir = path.join(nugetRoot, 'microsoft.web.webview2', version, 'lib');

    if (!fs.existsSync(packageDir)) {
        return null;
    }

    try {
        // Phase 1: check for .winmd directly in lib/ (flat layout)
        const directEntries = fs.readdirSync(packageDir, { withFileTypes: true });
        const directWinmd = directEntries.find(
            e => e.isFile() && e.name.toLowerCase().endsWith('.winmd'),
        );
        if (directWinmd) {
            return path.join(packageDir, directWinmd.name);
        }

        // Phase 2: search in TFM subdirectories (e.g. lib/net6.0-windows10.0.19041.0/)
        const tfmDirs = directEntries
            .filter(e => e.isDirectory())
            .map(e => path.join(packageDir, e.name));

        for (const tfmDir of tfmDirs) {
            const files = fs.readdirSync(tfmDir);
            const winmd = files.find(f => f.toLowerCase().endsWith('.winmd'));
            if (winmd) {
                return path.join(tfmDir, winmd);
            }
        }

        return null;
    } catch {
        return null;
    }
}

/**
 * Collect Win2D WinMD from the NuGet package cache.
 *
 * Win2D v1.2.0+ provides a single WinMD under
 * {@code lib/uap10.0/Microsoft.Graphics.Canvas.winmd}.
 */
export function collectWin2DWinmds(nugetRoot: string, version: string): string[] {
    const winmdPath = path.join(
        nugetRoot,
        'microsoft.graphics.win2d',
        version,
        'lib',
        'uap10.0',
        'Microsoft.Graphics.Canvas.winmd',
    );

    if (!fs.existsSync(winmdPath)) {
        return [];
    }

    return [winmdPath];
}

// ---------------------------------------------------------------------------
// Source directory resolution
// ---------------------------------------------------------------------------

/**
 * Determine the XAML source directory for a workspace root.
 *
 * Priority:
 *   1. {@code src/} at workspace root (conventional single-demo layout).
 *   2. Recursive search (max depth 4) for a directory containing {@code .xaml} files.
 *   3. Workspace root as final fallback.
 */
export function resolveSourceDir(workspaceRoot: string): string {
    // Priority 1: conventional src/ subdirectory
    const srcDir = path.join(workspaceRoot, 'src');
    if (fs.existsSync(srcDir) && fs.statSync(srcDir).isDirectory()) {
        return srcDir;
    }

    // Priority 2: recursive search for .xaml-containing directories
    const found = findXamlSrcDir(workspaceRoot);
    if (found) {
        return found;
    }

    // Priority 3: workspace root fallback
    return workspaceRoot;
}

/**
 * Search for the first child directory containing {@code .xaml} files.
 *
 * Performs a breadth-first recursive search up to {@code maxDepth} levels deep.
 * Returns the **deepest** directory that contains {@code .xaml} files (closest
 * to the actual source layout).
 *
 * Returns {@code null} when no such directory is found.
 */
export function findXamlSrcDir(workspaceRoot: string, maxDepth: number = 4): string | null {
    // Track the deepest matching directory so we prefer e.g.
    //   demo/hello/src  over  demo  over  workspaceRoot
    let bestMatch: string | null = null;
    let bestDepth = -1;

    function searchRecursive(currentDir: string, depth: number): void {
        if (depth > maxDepth) {
            return;
        }

        let entries: fs.Dirent[];
        try {
            entries = fs.readdirSync(currentDir, { withFileTypes: true });
        } catch {
            return; // permission error; skip this subtree
        }

        // Check if this directory directly contains .xaml files
        const hasXaml = entries.some(
            e => e.isFile() && e.name.toLowerCase().endsWith('.xaml'),
        );
        if (hasXaml && depth > bestDepth) {
            bestMatch = currentDir;
            bestDepth = depth;
        }

        // Recurse into subdirectories
        for (const entry of entries) {
            if (!entry.isDirectory()) {
                continue;
            }
            // Skip well-known non-source directories for efficiency
            const name = entry.name.toLowerCase();
            if (name === 'node_modules' || name === '.git' ||
                name === 'build' || name === '.xmake' || name === '.nuget') {
                continue;
            }
            searchRecursive(path.join(currentDir, entry.name), depth + 1);
        }
    }

    try {
        searchRecursive(workspaceRoot, 0);
        return bestMatch;
    } catch {
        return null;
    }
}

// ---------------------------------------------------------------------------
// Project WinMD discovery helpers
// ---------------------------------------------------------------------------

/**
 * Resolve the root directory where WinUI3 xmake places merged WinMD outputs.
 * Returns {@code workspaceRoot/build/.gens}.
 */
export function resolveBuildGensRoot(workspaceRoot: string): string {
    return path.join(workspaceRoot, 'build', '.gens');
}

/**
 * Locate the nearest project-level {@code xmake.lua} file.
 *
 * Strategy:
 * - When {@code xamlPath} is provided, walk UP from that file's parent directory
 *   looking for an {@code xmake.lua} (max 5 levels up).
 * - When not provided, check the workspace root for an {@code xmake.lua} first,
 *   then scan {@code demo/subdir/xmake.lua} subdirectories.
 *
 * @param workspaceRoot - The workspace root directory.
 * @param xamlPath - Optional path to a XAML file; used as the upward search seed.
 * @returns The full path to the nearest {@code xmake.lua}, or {@code null}.
 */
export function findProjectXmakeLua(workspaceRoot: string, xamlPath?: string): string | null {
    // Strategy 1: walk up from xamlPath
    if (xamlPath) {
        let dir = path.dirname(xamlPath);
        for (let i = 0; i < 5; i++) {
            const luaPath = path.join(dir, 'xmake.lua');
            if (fs.existsSync(luaPath)) {
                return luaPath;
            }
            const parent = path.dirname(dir);
            if (parent === dir) {
                break; // reached filesystem root
            }
            dir = parent;
        }
        return null;
    }

    // Strategy 2: workspace root xmake.lua
    const rootLua = path.join(workspaceRoot, 'xmake.lua');
    if (fs.existsSync(rootLua)) {
        return rootLua;
    }

    // Strategy 3: demo/*/xmake.lua
    try {
        const entries = fs.readdirSync(workspaceRoot, { withFileTypes: true });
        for (const entry of entries) {
            if (!entry.isDirectory()) {
                continue;
            }
            const demoLua = path.join(workspaceRoot, entry.name, 'xmake.lua');
            if (fs.existsSync(demoLua)) {
                return demoLua;
            }
        }
    } catch {
        // readdir failed; no match
    }

    return null;
}

/**
 * Parse an xmake.lua file for the first {@code target("...")} declaration and
 * return the target name.
 *
 * @param luaPath - Path to the xmake.lua file.
 * @returns The target name (e.g. {@code "demo.hello"}), or {@code null}.
 */
export function extractTargetName(luaPath: string): string | null {
    try {
        const content = fs.readFileSync(luaPath, 'utf-8');
        const match = content.match(/target\("([^"]+)"\)/);
        return match ? match[1] : null;
    } catch {
        return null;
    }
}

/**
 * Parse an xmake.lua file for a {@code set_values("winui3.namespace", "xxx")}
 * declaration and return the namespace value.
 *
 * @param luaPath - Path to the xmake.lua file.
 * @returns The namespace value, or {@code null} if not found.
 */
export function extractNamespaceFromXmakeLua(luaPath: string): string | null {
    try {
        const content = fs.readFileSync(luaPath, 'utf-8');
        const match = content.match(
            /set_values\("winui3\.namespace",\s*"(\w+)"\)/,
        );
        return match ? match[1] : null;
    } catch {
        return null;
    }
}

// ---------------------------------------------------------------------------
// Project WinMD discovery: build/.gens scanner
// ---------------------------------------------------------------------------

/**
 * Discover project-local WinMD files from winui3-xmake build outputs.
 *
 * Scans the {@code build/.gens/<target>/winmd_merged/} directory structure
 * and returns paths to WinMD files that match the given project namespace.
 *
 * Phase 1 — Guard: return empty when {@code build/.gens} is absent.
 * Phase 2 — Scan: iterate each target subdirectory for
 *           {@code winmd_merged/*.winmd} files.
 * Phase 3 — Prefer exact match: if a WinMD file is named
 *           {@code <namespace>.winmd}, return only those.
 * Phase 4 — Fallback: when no exact match exists, return all discovered
 *           WinMD files as ambiguous candidates.
 * Phase 5 — Deduplication: case-insensitive path normalization.
 *
 * @param workspaceRoot - The workspace root directory.
 * @param namespace     - The target namespace to match against WinMD
 *                        filenames (e.g., {@code "hello"} matches
 *                        {@code hello.winmd}).
 * @returns An array of normalised, deduplicated WinMD paths.
 */
export function discoverProjectWinmds(
    workspaceRoot: string,
    namespace: string,
): string[] {
    const gensRoot = resolveBuildGensRoot(workspaceRoot);

    // Phase 1: guard — build/.gens may not exist yet (pre-build)
    if (!fs.existsSync(gensRoot)) {
        return [];
    }

    const allCandidates: string[] = [];

    // Phase 2: scan each target subdirectory under build/.gens/
    let targetEntries: fs.Dirent[];
    try {
        targetEntries = fs.readdirSync(gensRoot, { withFileTypes: true });
    } catch {
        return [];
    }

    for (const targetEntry of targetEntries) {
        if (!targetEntry.isDirectory()) {
            continue;
        }

        const winmdMergedDir = path.join(
            gensRoot,
            targetEntry.name,
            'winmd_merged',
        );
        if (!fs.existsSync(winmdMergedDir)) {
            continue;
        }

        try {
            const files = fs.readdirSync(winmdMergedDir, {
                withFileTypes: true,
            });
            for (const file of files) {
                if (!file.isFile()) {
                    continue;
                }
                if (file.name.toLowerCase().endsWith('.winmd')) {
                    allCandidates.push(
                        path.normalize(path.join(winmdMergedDir, file.name)),
                    );
                }
            }
        } catch {
            // Permission / access error; skip this target directory
            continue;
        }
    }

    // Phase 5: case-insensitive deduplication
    const seen = new Set<string>();
    const unique: string[] = [];
    for (const candidate of allCandidates) {
        const lowerKey = candidate.toLowerCase();
        if (!seen.has(lowerKey)) {
            seen.add(lowerKey);
            unique.push(candidate);
        }
    }

    // Phase 3: prefer exact namespace match
    const exactMatches = unique.filter(
        p => path.basename(p, '.winmd') === namespace,
    );
    if (exactMatches.length > 0) {
        return exactMatches;
    }

    // Phase 4: ambiguous fallback — return all discovered WinMD files
    return unique;
}

// ---------------------------------------------------------------------------
// Project WinMD discovery: logging wrapper
// ---------------------------------------------------------------------------

/**
 * Wraps {@link discoverProjectWinmds} with diagnostic logging that explains
 * discovery decisions through the {@link DiscoverProjectWinmdsResult}
 * {@code warnings} array.
 *
 * Inspection logic (post-call):
 * - Cold-start (no {@code build/.gens}): returns empty result with **zero**
 *   warnings — this is a normal pre-build state, not an error.
 * - Exact match: at least one returned path's basename equals
 *   {@code namespace}; returns paths with no warnings.
 * - Ambiguous: returned paths exist but none match the namespace exactly;
 *   adds a warning with the candidate count.
 * - No results with structure: inspects the directory tree under
 *   {@code build/.gens} to distinguish "no winmd_merged dirs" from
 *   "no .winmd files in winmd_merged".
 *
 * @param workspaceRoot - The workspace root directory.
 * @param namespace     - The target namespace for candidate matching.
 * @returns A {@link DiscoverProjectWinmdsResult} with discovered paths and
 *          diagnostic warnings.
 */
export function discoverProjectWinmdsWithLogging(
    workspaceRoot: string,
    namespace: string,
): DiscoverProjectWinmdsResult {
    const gensRoot = resolveBuildGensRoot(workspaceRoot);

    // Phase 1: cold-start — build/.gens does not exist (normal pre-build).
    if (!fs.existsSync(gensRoot)) {
        return { paths: [], warnings: [] };
    }

    // Phase 2: call the scanner
    const paths = discoverProjectWinmds(workspaceRoot, namespace);

    // Phase 3: determine result quality from returned paths
    if (paths.length > 0) {
        const hasExactMatch = paths.some(
            p => path.basename(p, '.winmd') === namespace,
        );
        if (hasExactMatch) {
            // Exact match — successful discovery, no warning.
            return { paths, warnings: [] };
        }
        // Ambiguous — candidates exist but no exact namespace match.
        const msg =
            `Ambiguous project WinMD candidates for namespace '${namespace}': found ${paths.length} candidates`;
        console.log('[XAML LS]', msg);
        return { paths, warnings: [msg] };
    }

    // Phase 4: empty result — inspect directory structure for diagnostics.
    let hasWinmdMerged = false;
    let hasWinmdFiles = false;

    try {
        const targetDirs = fs
            .readdirSync(gensRoot, { withFileTypes: true })
            .filter(e => e.isDirectory());

        for (const targetDir of targetDirs) {
            const mergedDir = path.join(
                gensRoot,
                targetDir.name,
                'winmd_merged',
            );
            if (!fs.existsSync(mergedDir)) {
                continue;
            }
            hasWinmdMerged = true;

            try {
                const files = fs.readdirSync(mergedDir);
                if (files.some(f => f.toLowerCase().endsWith('.winmd'))) {
                    hasWinmdFiles = true;
                    break;
                }
            } catch {
                // Permission / access error; skip this target
                continue;
            }
        }
    } catch {
        // gensRoot read failed; fall through to warning logic below
    }

    if (!hasWinmdMerged) {
        const msg = 'No winmd_merged directories found under build/.gens';
        console.log('[XAML LS]', msg);
        return { paths: [], warnings: [msg] };
    }

    // winmd_merged exists but contains no .winmd files
    const msg = 'No .winmd files found in build/.gens/*/winmd_merged/';
    console.log('[XAML LS]', msg);
    return { paths: [], warnings: [msg] };
}

// ---------------------------------------------------------------------------
// Namespace extraction
// ---------------------------------------------------------------------------

/**
 * Extract the project root namespace from the workspace.
 *
 * Strategy (priority order):
 *   1. Parse demo-level xmake.lua files for {@code set_values("winui3.namespace", "xxx")}
 *   2. Parse the first XAML file found for {@code x:Class="namespace.Type"}
 *   3. Fallback to the workspace root basename
 */
export function extractNamespace(workspaceRoot: string): string {
    // Strategy 1: scan for demo/*/xmake.lua with set_values("winui3.namespace", ...)
    try {
        const entries = fs.readdirSync(workspaceRoot, { withFileTypes: true });
        for (const entry of entries) {
            if (!entry.isDirectory()) {
                continue;
            }

            // Try direct child xmake.lua (e.g. src/xmake.lua)
            const childDir = path.join(workspaceRoot, entry.name);
            const directLua = path.join(childDir, 'xmake.lua');
            if (fs.existsSync(directLua)) {
                const ns = extractNamespaceFromXmakeLua(directLua);
                if (ns !== null) {
                    return ns;
                }
            }

            // Try one level deeper (e.g. demo/hello/xmake.lua pattern)
            try {
                const subEntries = fs.readdirSync(childDir, { withFileTypes: true });
                for (const subEntry of subEntries) {
                    if (!subEntry.isDirectory()) {
                        continue;
                    }

                    const subDir = path.join(childDir, subEntry.name);
                    const subLua = path.join(subDir, 'xmake.lua');
                    if (!fs.existsSync(subLua)) {
                        continue;
                    }

                    const ns = extractNamespaceFromXmakeLua(subLua);
                    if (ns !== null) {
                        return ns;
                    }
                }
            } catch {
                // permission error; skip this subtree
                continue;
            }
        }
    } catch {
        // directory scan failed; proceed to fallbacks
    }

    // Strategy 2: find first XAML x:Class attribute
    try {
        const xamlDir = findXamlSrcDir(workspaceRoot);
        if (xamlDir) {
            const xamlFiles = fs.readdirSync(xamlDir, { withFileTypes: true })
                .filter(e => e.isFile() && e.name.toLowerCase().endsWith('.xaml'));
            for (const xamlFile of xamlFiles) {
                try {
                    const content = fs.readFileSync(
                        path.join(xamlDir, xamlFile.name),
                        'utf-8',
                    );
                    const classMatch = content.match(/x:Class="(\w+)\.(\w+)"/);
                    if (classMatch) {
                        return classMatch[1];
                    }
                } catch {
                    continue;
                }
            }
        }
    } catch {
        // XAML scan failed; proceed to fallback
    }

    // Strategy 3: workspace root basename
    return path.basename(workspaceRoot);
}

// ---------------------------------------------------------------------------
// NuGet cache version auto-discovery
// ---------------------------------------------------------------------------

/**
 * Discover the best NuGet package versions installed in the local cache.
 *
 * Scans {@code ~/.nuget/packages/} for installed WinAppSDK and WebView2
 * versions, selecting the highest version for each.
 *
 * Falls back to updated defaults when the cache is empty or inaccessible.
 */
export function discoverBestNuGetVersions(nugetRoot: string): NuGetVersions {
    const updatedDefaults: NuGetVersions = {
        winAppSdk: '1.8.0',
        webView2: '1.0.3912.50',
        foundation: '1.8.0',
        winui: '1.8.0',
        interactive: '1.8.0',
        win2d: '1.4.0',
    };

    if (!fs.existsSync(nugetRoot)) {
        return updatedDefaults;
    }

    try {
        const scanVersion = (prefix: string): string | null => {
            // Strategy 1: flat directory format (fallback folder layout)
            //   e.g. microsoft.windowsappsdk.foundation.1.8.260415000/
            const flatDirs = fs
                .readdirSync(nugetRoot, { withFileTypes: true })
                .filter(
                    e =>
                        e.isDirectory() &&
                        e.name.toLowerCase().startsWith(prefix),
                )
                .map(e => {
                    const versionStr = e.name.substring(prefix.length);
                    if (!versionStr || !versionStr.match(/^\d/)) {
                        return null;
                    }
                    return versionStr;
                })
                .filter((v): v is string => v !== null)
                .sort((a, b) => compareVersions(b, a));

            if (flatDirs.length > 0) {
                return flatDirs[0];
            }

            // Strategy 2: standard V3 NuGet cache layout
            //   e.g. microsoft.windowsappsdk.foundation/1.8.260415000/
            const prefixNoDot = prefix.replace(/\.$/, '');
            const pkgDir = path.join(nugetRoot, prefixNoDot);
            if (fs.existsSync(pkgDir)) {
                const versionDirs = fs
                    .readdirSync(pkgDir, { withFileTypes: true })
                    .filter(
                        e =>
                            e.isDirectory() &&
                            /^\d/.test(e.name),
                    )
                    .map(e => e.name)
                    .sort((a, b) => compareVersions(b, a));

                if (versionDirs.length > 0) {
                    return versionDirs[0];
                }
            }

            return null;
        };

        const bestAppSdk = scanVersion('microsoft.windowsappsdk.');
        const bestFoundation = scanVersion('microsoft.windowsappsdk.foundation.');
        const bestWinui = scanVersion('microsoft.windowsappsdk.winui.');
        const bestIxp = scanVersion('microsoft.windowsappsdk.interactiveexperiences.');
        const bestWebView2 = scanVersion('microsoft.web.webview2.');
        const bestWin2d = scanVersion('microsoft.graphics.win2d.');

        // Version format validation: NuGet uses SemVer (can have 3 or 4 segments)
        const isValidSemVerV3 = (v: string): boolean => /^\d+\.\d+\.\d+/.test(v);

        return {
            winAppSdk:
                bestAppSdk && isValidSemVerV3(bestAppSdk) ? bestAppSdk : updatedDefaults.winAppSdk,
            webView2:
                bestWebView2 && isValidSemVerV3(bestWebView2) ? bestWebView2 : updatedDefaults.webView2,
            foundation:
                bestFoundation && isValidSemVerV3(bestFoundation) ? bestFoundation : updatedDefaults.foundation,
            winui:
                bestWinui && isValidSemVerV3(bestWinui) ? bestWinui : updatedDefaults.winui,
            interactive:
                bestIxp && isValidSemVerV3(bestIxp) ? bestIxp : updatedDefaults.interactive,
            win2d:
                bestWin2d && isValidSemVerV3(bestWin2d) ? bestWin2d : updatedDefaults.win2d,
        };
    } catch {
        return updatedDefaults;
    }
}

/**
 * Build an {@link LspAssemblyContext} for the given workspace root.
 *
 * All internal errors are caught and result in a `null` return (graceful
 * degradation).
 */
export async function discoverLspContext(
    workspaceRoot: string,
): Promise<LspAssemblyContext | null> {
    try {
        // Phase 1: Windows SDK discovery and platform WinMD collection
        const sdkRoot = discoverWinSdkRoot();
        const sdkVersion = discoverWinSdkVersion(sdkRoot);
        const platformWinmds = collectPlatformWinmds(sdkRoot, sdkVersion);

        // Phase 2: version discovery (multi-strategy)
        //   a. parse xmake.lua → packages.config → NuGet cache auto-discovery
        const userProfile = process.env.USERPROFILE || process.env.HOME || '';
        const nugetRoot = path.join(userProfile, '.nuget', 'packages');
        const luaPath = path.join(workspaceRoot, 'xmake.lua');

        let versions: NuGetVersions;
        if (fs.existsSync(luaPath)) {
            // Strategy A: parse xmake.lua (with packages.config fallback)
            const xmakeVersions = parseXmakeLuaVersions(luaPath, workspaceRoot);
            // Attempt to enrich with sub-package versions from packages.config
            const configPath = path.join(workspaceRoot, 'packages.config');
            let pkgVersions: NuGetVersions | null = null;
            if (fs.existsSync(configPath)) {
                pkgVersions = parsePackagesConfigVersions(configPath);
            }
            versions = {
                winAppSdk: xmakeVersions.winAppSdk,
                webView2: xmakeVersions.webView2,
                foundation: pkgVersions?.foundation ?? xmakeVersions.winAppSdk,
                winui: pkgVersions?.winui ?? xmakeVersions.winAppSdk,
                interactive: pkgVersions?.interactive ?? xmakeVersions.winAppSdk,
                win2d: pkgVersions?.win2d ?? '1.4.0',
            };
        } else {
            // Strategy B: auto-discover from NuGet cache
            versions = discoverBestNuGetVersions(nugetRoot);
        }

        // Phase 3: NuGet cache WinMD collection (sub-package aware)
        const appSdkWinmds = collectSubPackageAppSdkWinmds(nugetRoot, versions);
        const webView2Winmd = discoverWebView2Winmd(nugetRoot, versions.webView2);
        const win2dWinmds = collectWin2DWinmds(nugetRoot, versions.win2d);

        // Phase 4: source directory resolution (recursive search)
        const sourceDir = resolveSourceDir(workspaceRoot);

        // Phase 5: namespace extraction (multi-strategy)
        const namespace = extractNamespace(workspaceRoot);

        // Phase 5.5: discover project-local WinMDs
        const projectResult = discoverProjectWinmdsWithLogging(
            workspaceRoot,
            namespace,
        );
        for (const warning of projectResult.warnings) {
            console.log('[XAML LS]', warning);
        }

        // Phase 6: aggregate results (global then project)
        const referenceAssemblies: string[] = [
            ...platformWinmds,
            ...appSdkWinmds,
            ...win2dWinmds,
        ];
        if (webView2Winmd !== null) {
            referenceAssemblies.push(webView2Winmd);
        }

        // Append project WinMDs (cross-deduplicated against global refs)
        const seen = new Set(referenceAssemblies.map(p => p.toLowerCase()));
        for (const projPath of projectResult.paths) {
            if (!seen.has(projPath.toLowerCase())) {
                seen.add(projPath.toLowerCase());
                referenceAssemblies.push(projPath);
            }
        }

        return {
            referenceAssemblies,
            winSdkRoot: sdkRoot,
            winSdkVersion: sdkVersion,
            namespace,
            sourceDir,
        };
    } catch {
        return null;
    }
}
