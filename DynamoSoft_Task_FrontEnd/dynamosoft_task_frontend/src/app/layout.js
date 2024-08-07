import 'bootstrap/dist/css/bootstrap.min.css';

export default function RootLayout({ children }) {
    return (
        <html lang="en">
            <body>
                <div className="container mt-5">
                    {children}
                </div>
            </body>
        </html>
    );
}