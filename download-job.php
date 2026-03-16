<?php
declare(strict_types=1);

$apiKey = $_SERVER['HTTP_X_API_KEY'] ?? '';
if ($apiKey !== 'corlanejobupload') {
    http_response_code(401);
    header('Content-Type: text/plain');
    echo "Unauthorized";
    exit;
}

$file = $_GET['file'] ?? '';
if (!is_string($file) || $file === '') {
    http_response_code(400);
    echo "Missing file";
    exit;
}

// prevent path traversal
if (!preg_match('/^[a-zA-Z0-9 _.-]+\.cor$/', $file)) {
    http_response_code(400);
    echo "Invalid file";
    exit;
}

$baseDir = realpath(__DIR__ . DIRECTORY_SEPARATOR . 'files');
$fullPath = realpath(__DIR__ . DIRECTORY_SEPARATOR . 'files' . DIRECTORY_SEPARATOR . $file);

if ($baseDir === false || $fullPath === false || strncmp($fullPath, $baseDir, strlen($baseDir)) !== 0) {
    http_response_code(404);
    echo "Not found";
    exit;
}

if (!is_file($fullPath)) {
    http_response_code(404);
    echo "Not found";
    exit;
}

header('Content-Type: application/octet-stream');
header('Content-Disposition: attachment; filename="' . basename($fullPath) . '"');
header('Content-Length: ' . filesize($fullPath));
readfile($fullPath);