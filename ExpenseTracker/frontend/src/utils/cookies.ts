import Cookies from "js-cookie";

//Utility functions that wrap the js-cookie library to simplify cookie operations (store, retrieve, delete).


// Wrapper function for setting cookies
// Using a wrapper makes it easy to swap cookie libraries later if needed
export function setCookie(
    name: string,           // Cookie name (e.g., "access_token")
    value: string,          // Cookie value (e.g., the JWT token)
    options?: Cookies.CookieAttributes  // Optional config (expires, secure, sameSite, etc.)
) {
    Cookies.set(name, value, options)
}

// Wrapper function for getting cookies
// Returns string | undefined (undefined if cookie doesn't exist)
export function getCookie(name: string) {
    return Cookies.get(name);
}

// Wrapper function for deleting cookies
export function deleteCookie(name: string) {
    Cookies.remove(name);
}