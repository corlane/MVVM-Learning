<?php
declare(strict_types=1);

header('Content-Type: text/plain; charset=UTF-8');

const API_KEY = 'corlanejobupload';
const UPLOAD_DIR = __DIR__ . '/files/';

function fail(int $statusCode, string $message): void
{
    http_response_code($statusCode);
    echo $message;
    exit;
}

if (($_SERVER['REQUEST_METHOD'] ?? '') !== 'POST') {
    fail(405, 'Method not allowed. Use POST.');
}

$apiKey = $_SERVER['HTTP_X_API_KEY'] ?? '';
if (!hash_equals(API_KEY, $apiKey)) {
    fail(401, 'Unauthorized.');
}

if (!is_dir(UPLOAD_DIR)) {
    fail(500, 'Upload directory does not exist: ' . UPLOAD_DIR);
}

if (!is_writable(UPLOAD_DIR)) {
    fail(500, 'Upload directory is not writable: ' . UPLOAD_DIR);
}

if (!isset($_FILES['jobFile'])) {
    fail(400, 'No file uploaded. Expected field "jobFile".');
}

$f = $_FILES['jobFile'];

if (!isset($f['error'], $f['tmp_name'], $f['name'])) {
    fail(400, 'Invalid upload payload.');
}

if ($f['error'] !== UPLOAD_ERR_OK) {
    fail(400, 'Upload failed with error code: ' . (string)$f['error']);
}

if (!is_uploaded_file($f['tmp_name'])) {
    fail(400, 'Upload did not pass is_uploaded_file check.');
}

$originalName = (string)$f['name'];
$base = pathinfo($originalName, PATHINFO_FILENAME);

// sanitize: keep safe chars only
$base = preg_replace('/[^A-Za-z0-9 _\-\(\)]+/', '', $base) ?? 'job';
$base = trim($base);
if ($base === '') {
    $base = 'job';
}

// Always append timestamp (UTC)
$timestamp = gmdate('Ymd_His');
$finalName = $base . '_' . $timestamp . '.cor';
$targetPath = UPLOAD_DIR . $finalName;

// Extremely unlikely now, but handle same-second collision just in case
if (file_exists($targetPath)) {
    $finalName = $base . '_' . $timestamp . '_2.cor';
    $targetPath = UPLOAD_DIR . $finalName;
}

if (!move_uploaded_file($f['tmp_name'], $targetPath)) {
    fail(500, 'Failed to save uploaded file.');
}

http_response_code(200);
echo 'OK: ' . $finalName;