/**
 * VS Code 扩展发布前准备脚本。
 * 构建 .NET LSP 服务器并将产物复制到扩展的 server/ 目录。
 * 
 * 执行流程：
 * 1. dotnet publish XamlCompiler.LanguageServer.csproj → server/bin/
 * 2. 复制 publish 产物到 server/（仅必要的 DLL 和运行时文件）
 */

const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

const configIndex = process.argv.indexOf('--configuration');
let configuration = 'Release';
if (configIndex !== -1 && configIndex + 1 < process.argv.length) {
  const value = process.argv[configIndex + 1];
  if (value === 'Debug' || value === 'Release') {
    configuration = value;
  } else {
    console.warn(`Unknown configuration "${value}", defaulting to Release`);
  }
}

const serverProjectDir = path.resolve(__dirname, '..', '..', '..', 'src', 'XamlCompiler.LanguageServer');
const serverOutputDir = path.resolve(__dirname, '..', 'server');
const publishDir = path.join(serverProjectDir, 'bin', configuration, 'net472', 'publish');

function clean(dir) {
    if (fs.existsSync(dir)) {
        fs.rmSync(dir, { recursive: true, force: true });
    }
}

function copyDir(src, dest) {
    if (!fs.existsSync(dest)) {
        fs.mkdirSync(dest, { recursive: true });
    }
    const entries = fs.readdirSync(src, { withFileTypes: true });
    for (const entry of entries) {
        const srcPath = path.join(src, entry.name);
        const destPath = path.join(dest, entry.name);
        if (entry.isDirectory()) {
            copyDir(srcPath, destPath);
        } else {
            fs.copyFileSync(srcPath, destPath);
        }
    }
}

console.log('Building XAML Language Server...');

// 清理之前的产物
clean(serverOutputDir);
clean(path.join(serverProjectDir, 'bin', configuration));

// dotnet publish
execSync(`dotnet publish -c ${configuration} -o publish`, {
    cwd: serverProjectDir,
    stdio: 'inherit'
});

// 复制产物到 server/ 目录
const actualPublishDir = path.join(serverProjectDir, 'publish');
if (fs.existsSync(actualPublishDir)) {
    copyDir(actualPublishDir, serverOutputDir);
    console.log(`Server published to ${serverOutputDir}`);
} else {
    console.warn('Publish directory not found, falling back to bin/Release/net472/publish');
    if (fs.existsSync(publishDir)) {
        copyDir(publishDir, serverOutputDir);
        console.log(`Server published to ${serverOutputDir}`);
    } else {
        console.error('Failed to find published server output');
        process.exit(1);
    }
}

console.log('XAML Language Server build complete.');
