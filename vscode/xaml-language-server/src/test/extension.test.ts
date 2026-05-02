import * as assert from 'assert';
import * as vscode from 'vscode';

suite('XAML Language Server Extension Test Suite', () => {

    test('Extension should be present', () => {
        assert.ok(vscode.extensions.getExtension('xaml-compiler.xaml-language-server'));
    });

    test('Extension should activate', async () => {
        const ext = vscode.extensions.getExtension('xaml-compiler.xaml-language-server');
        if (ext) {
            await ext.activate();
            assert.ok(ext.isActive);
        }
    });
});
