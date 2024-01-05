# EazyNotes Full Stack

Backend and Desktop Front-end repository for EazyNotes, an End-to-End-Encrypted privacy-preserving note-taking app.
**Please note that the repository name EazyNotesAPI is a slight misnomer, as originally a separation of the API from the WPF Front-end was intended.**

## Features
- **End-to-End Encryption**: Notes are encrypted on the client side using 
- **Notes in Topics**: Notes are neatly organized into topics, each with their own color and icon.
- **Auto-Sync**: Automatically syncs across all devices in real-time.
- **Android Client**: There is also a [Flutter client](https://www.github.com/coffeecoding/EasyNotes) for this app, which for now works on Android.

## Technology Stack
- **Frontend**: WPF (Windows), Flutter (Android).
- **Backend**: ASP.NET Core Web API
- **Database**: MySQL Database
- **Encryption**: Password-based key derivation (PBKDF2), RSA and AES 256 bit encryption.

## Roadmap

A potential rewrite of the whole stack with a single cross-platform Flutter client (Desktop and Mobile) is planned.

