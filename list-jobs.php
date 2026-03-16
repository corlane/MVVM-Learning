<?php
declare(strict_types=1);

$apiKey = $_SERVER['HTTP_X_API_KEY'] ?? '';
if ($apiKey !== 'corlanejobupload') {
    http_response_code(401);
    header('Content-Type: application/json');
    echo json_encode(['error' => 'Unauthorized']);
    exit;
}

$dir = __DIR__ . DIRECTORY_SEPARATOR . 'files' . DIRECTORY_SEPARATOR;
$files = glob($dir . '*.cor') ?: [];

$result = [];
foreach ($files as $path) {
    $name = basename($path);

    // extra safety
    if (!preg_match('/^[a-zA-Z0-9 _.-]+\.cor$/', $name)) {
        continue;
    }

    $result[] = [
        'fileName' => $name,
        'sizeBytes' => filesize($path) ?: 0,
        'lastWriteUtc' => gmdate('c', filemtime($path) ?: time())
    ];
}

usort($result, fn($a, $b) => strcmp($b['lastWriteUtc'], $a['lastWriteUtc'])); // newest first

header('Content-Type: application/json');
echo json_encode($result, JSON_PRETTY_PRINT);