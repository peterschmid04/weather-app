// The frontend is Docker-first and uses relative API paths.
// Create React App's proxy forwards these paths to backend:5122 inside Docker.
export const buildApiUrl = (path) => {
  return path.startsWith("/") ? path : `/${path}`;
};
